# CSharpRabbitMQWithSagas

The idea of project is to implement a client, shop and a warehouse that communicate with each other through queues.

Client can ask for goods, and if available he can get them. If amount is not available, then order is rejected.

Warehouse can add goods to itself. In a warehouse there are two places for goods: free and reserved. Free goods are waiting for someone to place order on them. 
Should there arrive an order, they go into reserved section.
Reserved goods are waiting to be confirmed by the client. 

Shop is the intermediary between the two. In Shop.cs there is State machine implemented with 3 states.

Client has to confirm in 15 seconds his order or it will be annuled. In such case reserved goods will become free again.
