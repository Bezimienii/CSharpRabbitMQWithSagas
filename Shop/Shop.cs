// See https://aka.ms/new-console-template for more information
using MassTransit;
using Messages;

class OrderReservation : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string ID { get; set; }
    public int Amount { get; set; }
    public Guid? TimeoutId { get; set; }
}

class ShopOrdersManager : MassTransitStateMachine<OrderReservation>
{
    public State Unconfirmed { get; private set; }
    public State ClientConfirmed { get; private set; }
    public State WarehouseConfirmed { get; private set; }

    public Event<ReservationRequest> ReservationRequest { get; private set; }
    public Event<ConfirmationRequest> ConfirmationRequest { get; private set; }
    public Event<PositiveConfirmationResponse> PositiveConfirmationResponse { get; private set; }
    public Event<NegativeConfirmationResponse> NegativeConfirmationResponse { get; private set; }
    public Event<CheckAmountRequest> CheckAmountRequest { get; private set;}
    public Event<AmountAvailableResponse> AmountAvailableResponse { get; private set; }
    public Event<AmountNotAvailableResponse> AmountNotAvailableResponse { get; private set; }
    public Event<Messages.Timeout> TimeoutEvent { get; private set; }
    public Schedule<OrderReservation, Messages.Timeout> TO { get; private set; }

    public ShopOrdersManager()
    {
        InstanceState(x => x.CurrentState);

        Event(() => ReservationRequest,
            x => x.CorrelateBy(
                s => s.ID,
                ctx => ctx.Message.ID).SelectId(context => Guid.NewGuid()));

        Schedule(() => TO,
                x => x.TimeoutId,
                x => { x.Delay = TimeSpan.FromSeconds(15); }
            );

        Initially(
            When(ReservationRequest)
            .Schedule(TO, ctx => new Messages.Timeout() { CorrelationId = ctx.Saga.CorrelationId })
            .Then(ctx => ctx.Saga.ID = ctx.Message.ID)
            .Then(ctx => ctx.Saga.Amount = ctx.Message.Amount)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"Order for {ctx.Message.ID} for goods in the quantity of {ctx.Message.Amount}\n"))
            .Respond(ctx => { return new ConfirmationRequest() { CorrelationId = ctx.Saga.CorrelationId, ID = ctx.Saga.ID }; })
            .Respond(ctx => { return new CheckAmountRequest() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount };  })
            .TransitionTo(Unconfirmed)
            );

        During(Unconfirmed,
            When(TimeoutEvent)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A timeout has occured for client with {ctx.Saga.ID} on order with ID: {ctx.Saga.CorrelationId}\n"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize(),

            When(PositiveConfirmationResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A client with ID {ctx.Saga.ID} has confirmed his choice"))
            .Unschedule(TO)
            .TransitionTo(ClientConfirmed),

            When(NegativeConfirmationResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A client with ID {ctx.Saga.ID} has not confirmed"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize(),

            When(AmountAvailableResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A warehouse has the amount requested by the client {ctx.Saga.ID} " +
            $"in order {ctx.Saga.CorrelationId}"))
            .TransitionTo(WarehouseConfirmed),

            When(AmountNotAvailableResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A warehouse does not the amount requested by the client {ctx.Saga.ID} " +
            $"in order {ctx.Saga.CorrelationId}"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize()
            );

        During(ClientConfirmed,
            When(AmountAvailableResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A warehouse has the amount requested by the client {ctx.Saga.ID} " +
            $"in order {ctx.Saga.CorrelationId}"))
            .Respond(ctx => { return new AcceptOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize(),

            When(AmountNotAvailableResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A warehouse does not the amount requested by the client {ctx.Saga.ID} " +
            $"in order {ctx.Saga.CorrelationId}"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize()
            );

        During(WarehouseConfirmed,
            When(TimeoutEvent)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A timeout has occured for client with {ctx.Saga.ID} on order with ID: {ctx.Saga.CorrelationId}\n"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize(),

            When(PositiveConfirmationResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A client with ID {ctx.Saga.ID} has confirmed his choice"))
            .Respond(ctx => { return new AcceptOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Unschedule(TO)
            .Finalize(),

            When(NegativeConfirmationResponse)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"A client with ID {ctx.Saga.ID} has not confirmed"))
            .Respond(ctx => { return new RejectOrder() { CorrelationId = ctx.Saga.CorrelationId, Amount = ctx.Saga.Amount, ID = ctx.Saga.ID }; })
            .Finalize()
            );

        SetCompletedWhenFinalized();

    }

}

class Shop
{
    public static void Main(string[] args)
    {
        var repository = new InMemorySagaRepository<OrderReservation>();
        var saga = new ShopOrdersManager();

        var bus = Bus.Factory.CreateUsingRabbitMq(
            sbc =>
            {
                sbc.Host(
                     new Uri("rabbitmq://localhost/184543"),
                     h => { h.Username("guest"); h.Password("guest"); });
                sbc.ReceiveEndpoint("saga",
                    ep => ep.StateMachineSaga(saga, repository));
                sbc.UseInMemoryScheduler();
            }
            );
            bus.Start();
            Console.WriteLine("Shop is open");
            Console.ReadKey();
            bus.Stop();
            Console.WriteLine("Shop is closed");
    }
}

