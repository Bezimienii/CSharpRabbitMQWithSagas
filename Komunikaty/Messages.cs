// See https://aka.ms/new-console-template for more information
using MassTransit;

namespace Messages
{
    public interface ISendReservationRequest
    {    
        string ID { get; set; }
        int Amount { get; set; }
    }
    public class SendReservationRequest : ISendReservationRequest
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
    }
    public class ConfirmationRequest : IConfirmationRequest
    {
        public Guid CorrelationId { get; set; }
    }
    public interface IConfirmation : CorrelatedBy<Guid>
    {
        bool HasConfirmed { get; set; }
    }

    public class Confirmation : IConfirmation
    {
        public bool HasConfirmed { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface ICheckAmountRequest : CorrelatedBy<Guid>
    {
    }
    public class CheckAmountRequest : ICheckAmountRequest
    {
        public Guid CorrelationId { get; set; }
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
        string ID { get; set; };
    }
    public class AcceptOrder: IAcceptOrder
    {
        public bool HasAcceptedOrder { get; set; }
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
