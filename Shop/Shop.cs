// See https://aka.ms/new-console-template for more information
using MassTransit;
using Messages;

class OrderReservation : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }
    public string ID { get; set; }
    public bool HasConfirmed { get; set; }
    public bool IsAmountAvailable { get; set; }
    public bool HasAcceptedOrder { get; set; }
    public int Amount { get; set; }
    public Guid? TimeoutId { get; set; }
}

class ShopOrdersManager : MassTransitStateMachine<OrderReservation>
{
    public State Unconfirmed { get; private set; }
    public State ClientConfirmed { get; private set; }
    public State WarehouseConfirmed { get; private set; }

    public Event<ReservationRequest> SendReservationEvent { get; private set; }
    public Event<ConfirmationRequest> ConfirmationRequestEvent { get; private set; }
    public Event<ConfirmationResponse> ConfirmationEvent { get; private set; }
    public Event<CheckAmountRequest> CheckAmountRequestEvent { get; private set;}
    public Event<CheckAmountResponse> CheckAmountResponseEvent { get; private set; }
    public Event<Messages.Timeout> TimeoutEvent { get; private set; }
    public Schedule<OrderReservation, Messages.Timeout> TO { get; private set; }

    public ShopOrdersManager()
    {
        InstanceState(x => x.CurrentState);

        Event(() => SendReservationEvent,
            x => x.CorrelateBy(
                s => s.ID,
                ctx => ctx.Message.ID).SelectId(context => Guid.NewGuid()));

        Schedule(() => TO,
                x => x.TimeoutId,
                x => { x.Delay = TimeSpan.FromSeconds(15)}
            );

        Initially(
            When(SendReservationEvent)
            .Schedule(TO, ctx => new Messages.Timeout() { CorrelationId = ctx.Instance.CorrelationId })
            .Then(ctx => ctx.Instance.ID = ctx.Data.ID)
            .Then(ctx => ctx.Instance.Amount = ctx.Data.Amount)
            .ThenAsync(ctx => Console.Out.WriteLineAsync($"\n Order for {ctx.Data.ID} for goods in the quantity of {ctx.Data.Amount}"))
            .Respond(ctx => { return new ConfirmationRequest() { CorrelationId = ctx.Instance.CorrelationId, ID = ctx.Instance.ID }; })
            .Respond(ctx => { return new CheckAmountRequest() { CorrelationId = ctx.Instance.CorrelationId, Amount = ctx.Instance.Amount };  })
            .TransitionTo(Unconfirmed)
            );
    }

}

