using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;

namespace Evaluant.Uss.WCFService
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PersistenceEngineBehaviorAttribute : Attribute, IOperationBehavior
    {
        #region IOperationBehavior Members

        public void AddBindingParameters(OperationDescription operation, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            serializer.DataContractSurrogate = new DataContractSurrogate();

        }

        public void ApplyClientBehavior(OperationDescription operation, System.ServiceModel.Dispatcher.ClientOperation clientOperation)
        {
            var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            serializer.DataContractSurrogate = new DataContractSurrogate();

        }

        public void ApplyDispatchBehavior(OperationDescription operation, System.ServiceModel.Dispatcher.DispatchOperation dispatchOperation)
        {
            var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            serializer.DataContractSurrogate = new DataContractSurrogate();
        }

        public void Validate(OperationDescription operation)
        {
            var serializer = operation.Behaviors.Find<DataContractSerializerOperationBehavior>();
            serializer.DataContractSurrogate = new DataContractSurrogate();
        }

        #endregion
    }
}
