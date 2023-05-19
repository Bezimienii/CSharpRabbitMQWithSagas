// See https://aka.ms/new-console-template for more information
using MassTransit;
using Messages;

class WarehouseConsumer : IConsumer<ICheckAmountRequest>, IConsumer<IAcceptOrder>, IConsumer<IRejectOrder>
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
                context.RespondAsync(new AmountAvailableResponse() { CorrelationId = context.Message.CorrelationId});

            }
            else
            {
                Console.Out.WriteLineAsync($"A client has filed a request for goods, but there are not enough goods!");
                context.RespondAsync(new AmountNotAvailableResponse() { CorrelationId = context.Message.CorrelationId });
            }
        }));
        
    }

    public Task Consume(ConsumeContext<IAcceptOrder> context)
    {
        return Task.Run(() =>
        {
            this.ReservedGoods -= context.Message.Amount;
            Console.Out.WriteLineAsync($"The order has been realized");
        });
    }

    public Task Consume(ConsumeContext<IRejectOrder> context)
    {
        return Task.Run(() =>
        {
            this.ReservedGoods -= context.Message.Amount;
            this.FreeGoods += context.Message.Amount;
            Console.Out.WriteLineAsync($"The order has not been realized");
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
                    new Uri("rabbitmq://localhost/184543"),
                    h => { h.Username("guest"); h.Password("guest"); }
                );
                sbc.ReceiveEndpoint(WarehouseName,
                    ep => ep.Instance(warehouse));
            }
        );
        Console.WriteLine("Warehouse is open\n");
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