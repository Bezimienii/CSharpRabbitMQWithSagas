// See https://aka.ms/new-console-template for more information
using MassTransit;

namespace Messages
{
    public interface IReservationRequest
    {    
        string ID { get; set; }
        int Amount { get; set; }
    }
    public class ReservationRequest : IReservationRequest
    {
        public string ID { get; set; }
        public int Amount { get; set; }
    }
    public interface IConfirmationCheck : CorrelatedBy<Guid> 
    {
        string ID { get; set; }
    }
    public class ConfirmationCheck : IConfirmationCheck
    {
        public string ID { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface IConfirmationRequest : CorrelatedBy<Guid>
    {
        string ID { get; set; }
    }
    public class ConfirmationRequest : IConfirmationRequest
    {
        public string ID { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface IConfirmationResponse : CorrelatedBy<Guid>
    {
        bool HasConfirmed { get; set; }
    }

    public class ConfirmationResponse : IConfirmationResponse
    {
        public bool HasConfirmed { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface ICheckAmountRequest : CorrelatedBy<Guid>
    {
        public int Amount { get; set; }
    }
    public class CheckAmountRequest : ICheckAmountRequest
    {
        public Guid CorrelationId { get; set; }
        public int Amount { get; set; }
    }
    public interface ICheckAmountResponse : CorrelatedBy<Guid>
    {
        bool IsAmountAvailable { get; set; }
    }
    public class CheckAmountResponse : ICheckAmountResponse
    {
        public bool IsAmountAvailable { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface IAcceptOrder : CorrelatedBy<Guid>
    {
        bool HasAcceptedOrder { get; set; }
        int Amount { get; set; }
        string ID { get; set; }
    }
    public class AcceptOrder: IAcceptOrder
    {
        public bool HasAcceptedOrder { get; set; }
        public int Amount { get; set; }
        public string ID { get; set; }
        public Guid CorrelationId { get; set; }
    }

    public interface ITimeout : CorrelatedBy<Guid>
    {
    }
    public class Timeout : ITimeout
    {
        public Guid CorrelationId { get; set; }
    }



}
