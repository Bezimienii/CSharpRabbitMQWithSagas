// See https://aka.ms/new-console-template for more information
using MassTransit;
using Messages;

class WarehouseConsumer : IConsumer<ICheckAmountRequest>, IConsumer<IAcceptOrder>
{
    public int FreeGoods { get; set; } = 0;
    public int ReservedGoods { get; set; } = 0; 
    public WarehouseConsumer()
    {
    }
    public Task Consume(ConsumeContext<ICheckAmountRequest> context)
    {
        return (Task.Run(() =>
        {
            if (context.Message.Amount <= this.FreeGoods)
            {
                this.FreeGoods -= context.Message.Amount;
                this.ReservedGoods += context.Message.Amount;
                Console.Out.WriteLineAsync($"A client has filed a request for goods, and it can be realized!");
                context.RespondAsync(new CheckAmountResponse() { CorrelationId = context.Message.CorrelationId, IsAmountAvailable = true });

            }
            else
            {
                Console.Out.WriteLineAsync($"A client has filed a request for goods, but there are not enough goods!");
                context.RespondAsync(new CheckAmountResponse() { CorrelationId = context.Message.CorrelationId, IsAmountAvailable = false });
            }
        }));
        
    }

    public Task Consume(ConsumeContext<IAcceptOrder> context)
    {
        return Task.Run(() =>
        {
            this.ReservedGoods -= context.Message.Amount;
            if(!context.Message.HasAcceptedOrder)
            {
                this.FreeGoods += context.Message.Amount;
            }
            String OrderRealizationStatus = context.Message.HasAcceptedOrder ? "been realized" : "not been realized";
            Console.Out.WriteLineAsync($"The order {context.Message.CorrelationId} has {OrderRealizationStatus}");
        });
    }
}

class Warehouse
{
    static void Main(string[] args)
    {
        String WarehouseName = "Warehouse";
        var warehouse = new WarehouseConsumer();
        var bus = Bus.Factory.CreateUsingRabbitMq(
            sbc =>
            {
                sbc.Host(
                    new Uri("rabbitmq://localhost/username"),
                    h => { h.Username("guest"); h.Password("guest"); }
                );
                sbc.ReceiveEndpoint(WarehouseName,
                    ep => ep.Instance(warehouse));
            }
        );
        bus.Start();
        while (true)
        {
            if (Console.ReadKey().Key == ConsoleKey.A)
            {
                Console.WriteLine("How many goods do you want to add to the warehouse?");

                int quantity = 0;
                try
                {
                    quantity = Convert.ToInt32(Console.ReadLine());
                    warehouse.FreeGoods += quantity;
                    Console.WriteLine($"State of warehouse\nAmount of free goods:{warehouse.FreeGoods}\nAmount of reserved goods:{warehouse.ReservedGoods}\nIn Total: " +
                        $"{warehouse.FreeGoods+warehouse.ReservedGoods}");
                }
                catch (Exception)
                {
                    Console.WriteLine("\nThere should be a number entered");
                }
            }
        }
    }
}