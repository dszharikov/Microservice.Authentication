﻿using System.Text;
using System.Text.Json;
using IdentityModule.AsyncDataServices.MessageBusClients;
using IdentityModule.Dtos;
using RabbitMQ.Client;

namespace Password.AsyncDataServices;

public class MessageBusClient : IMessageBusClient
{
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageBusClient(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory() { HostName = _configuration["RabbitMQHost"], 
            Port = int.Parse(_configuration["RabbitMQPort"]) };

        try
        {
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            _channel.ExchangeDeclare(exchange: "userCreated", type: ExchangeType.Direct);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutDown;

            System.Console.WriteLine("--> Connected to MessageBus");
            
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"--> Could not connect to the Message Bus {ex.Message}");
        }
    }

    private void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "userCreated", 
            routingKey: "user",
            basicProperties: null,
            body: body);

        System.Console.WriteLine($"--> We have send {message}");
    }

    public void Dispose()
    {
        System.Console.WriteLine("--> MessageBus Disposed");

        if (_channel.IsOpen)
        {
            _channel.Close();
            _connection.Close();
        }
    }

    private void RabbitMQ_ConnectionShutDown(object? sender, ShutdownEventArgs e)
    {
        System.Console.WriteLine("--> RabbitMQ Connection Shutdown");
    }

    public void PublishNewUser(PublishUserDto userPublishDto)
    {
        var message = JsonSerializer.Serialize(userPublishDto);

        if (_connection.IsOpen)
        {
            System.Console.WriteLine("--> RabbitMQ Connection Open, sending message...");

            SendMessage(message);
        }
        else
        {
            System.Console.WriteLine("--> RabbitMQ Connection is Closed, not sending");
        }
    }
}