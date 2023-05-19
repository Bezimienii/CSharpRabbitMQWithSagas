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
    public interface IPositiveConfirmationResponse : CorrelatedBy<Guid>
    {
    }

    public class PositiveConfirmationResponse : IPositiveConfirmationResponse
    {
        public Guid CorrelationId { get; set; }
    }
    public interface INegativeConfirmationResponse : CorrelatedBy<Guid>
    {
    }

    public class NegativeConfirmationResponse : INegativeConfirmationResponse
    {
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
    public interface IAmountAvailableResponse : CorrelatedBy<Guid>
    {
    }
    public class AmountAvailableResponse : IAmountAvailableResponse
    {
        public Guid CorrelationId { get; set; }
    }
    public interface IAmountNotAvailableResponse : CorrelatedBy<Guid>
    {
    }
    public class AmountNotAvailableResponse : IAmountNotAvailableResponse
    {
        public Guid CorrelationId { get; set; }
    }
    public interface IAcceptOrder : CorrelatedBy<Guid>
    {
        int Amount { get; set; }
        string ID { get; set; }
    }
    public class AcceptOrder: IAcceptOrder
    {
        public int Amount { get; set; }
        public string ID { get; set; }
        public Guid CorrelationId { get; set; }
    }
    public interface IRejectOrder: CorrelatedBy<Guid>
    {
        public int Amount { get; set; }
        public string ID { get; set; }
    }
    public class RejectOrder: IRejectOrder
    {
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
