using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CarmineCrystal.Networking
{
	public abstract class MessageProcessingModule
	{
		public abstract Type AcceptedType { get; }
		public void Process(Message ProcessTarget, NetworkClient Sender)
		{
			if (ProcessTarget.GetType() == AcceptedType || ProcessTarget.GetType().GetTypeInfo().IsSubclassOf(AcceptedType))
			{
				Run(ProcessTarget, Sender);
			}
		}

		protected abstract void Run(Message ReceivedMessage, NetworkClient Sender);
	}

	public abstract class GenericMessageProcessingModule<T> : MessageProcessingModule where T : Message
	{
		public override Type AcceptedType
		{
			get
			{
				return typeof(T);
			}
		}

		protected override void Run(Message RunTarget, NetworkClient Sender)
		{
			Run((T)RunTarget, Sender);
		}

		protected abstract void Run(T RunTarget, NetworkClient Sender);
	}

	public abstract class GenericRequestProcessingModule<T, U> : MessageProcessingModule where T : Request where U : Response
	{
		public override Type AcceptedType
		{
			get
			{
				return typeof(T);
			}
		}

		protected override void Run(Message RunTarget, NetworkClient Sender)
		{
			U Returned = Run((T)RunTarget, Sender);
			if (Returned != null)
			{
				Returned.ID = ((Request)RunTarget).ID;
				Sender.Send(Returned);
			}
		}

		protected abstract U Run(T RunTarget, NetworkClient Sender);
	}

	public class DelegateMessageProcessingModule<T> : GenericMessageProcessingModule<T> where T : Message
	{
		private Action<T,NetworkClient> Processor;

		public DelegateMessageProcessingModule(Action<T,NetworkClient> Processor)
		{
			this.Processor = Processor;
		}

		protected override void Run(T RunTarget, NetworkClient Sender)
		{
			Processor(RunTarget, Sender);
		}
	}
}
