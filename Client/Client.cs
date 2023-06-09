﻿// See https://aka.ms/new-console-template for more information
using MassTransit;
using Messages;

class ClientConsumer : IConsumer<IConfirmationRequest>, IConsumer<IAcceptOrder>, IConsumer<IRejectOrder>
{
    private String ClientName;
    public ClientConsumer(String NameOfClient)
    {
        this.ClientName = NameOfClient;
    }
    public Task Consume(ConsumeContext<IConfirmationRequest> context)
    {
        if(context.Message.ID == this.ClientName)
        {
            Console.WriteLine($"Should the order with ID {context.Message.CorrelationId} be accepted?");
            bool odp = false;
            if(Console.ReadKey().Key == ConsoleKey.T)
            {
                odp = true;
            }
            if( odp )
            {
                return Task.Run(() =>
                {
                    context.RespondAsync(new PositiveConfirmationResponse() { CorrelationId = context.Message.CorrelationId });
                });
            }
            else
            {
                return Task.Run(() =>
                {
                    context.RespondAsync(new NegativeConfirmationResponse() { CorrelationId = context.Message.CorrelationId });
                });
            }
        }
        else
        {
            return Task.Run(() => { });
        }
    }

    public Task Consume(ConsumeContext<IAcceptOrder> context)
    {
        if(context.Message.ID == this.ClientName)
        {
            return Console.Out.WriteLineAsync($"Order has been accepted");
        }
        else
        {
            return Task.Run(() => { });
        }
    }

    public Task Consume(ConsumeContext<IRejectOrder> context)
    {
        if (context.Message.ID == this.ClientName)
        {
            return Console.Out.WriteLineAsync($"Order has been rejected for order {context.Message.CorrelationId}");
        }
        else
        {
            return Task.Run(() => { });
        }
    }
}

class Client 
{
    static void Main(string[] args)
    {
        String ClientName = "Client";
        var queue = new ClientConsumer(ClientName);
        var bus = Bus.Factory.CreateUsingRabbitMq(
            sbc =>
            {
                sbc.Host(
                    new Uri("rabbitmq://localhost/184543"),
                    h => { h.Username("guest"); h.Password("guest"); }
                );
                sbc.ReceiveEndpoint(ClientName,
                    ep => ep.Instance(queue));
            }
        );
        Console.WriteLine("Client has connected\n");
        bus.Start();
        while(true)
        {
            if(Console.ReadKey().Key == ConsoleKey.B)
            {
                Console.WriteLine("In what quantity do you want the goods?");

                int quantity = 0;
                try
                {
                    quantity = Convert.ToInt32(Console.ReadLine());
                    bus.Publish(new ReservationRequest() { ID = ClientName, Amount = quantity });
                    Console.WriteLine();
                }
                catch(Exception) 
                {
                    Console.WriteLine("\nThere should be a number entered");
                }
            }
        }
    }
}
