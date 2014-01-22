using System;
using System.Globalization;

using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
//using Evaluant.Uss.Common;
//using Evaluant.Uss.MetaData;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.Remoting
{
	/// <summary>
	/// Description résumée de RemoteProvider.
	/// </summary>
	public class RemoteProvider : IPersistenceProvider
	{

		public RemoteProvider()
		{
		}

		public void InitializeConfiguration()
		{
		}

		public IPersistenceEngine CreatePersistenceEngine()
		{
			try
			{
				TcpChannel chan = new TcpChannel();
				ChannelServices.RegisterChannel(chan);
				RemoteController _Instance = (RemoteController)Activator.GetObject(
					typeof(RemoteController),
					"tcp://" + _Host + ":" + _Port.ToString() + "/Evaluant.Uss.Remoting.RemoteController");
			
				if (_Instance == null) 
					throw new PersistenceEngineException("Could not connect to server");

				return new RemotingPersistenceEngine(_Instance);
			}
			catch(Exception e)
			{
				throw new PersistenceEngineException("Could not connect to server", e);
			}
		}

		public void RegisterMetaData(Evaluant.Uss.MetaData.IMetaData[] metadata)
		{
		}

		private string  _Host;
		public string Host
		{
			get { return _Host; }
			set { _Host = value; }
		}

		private int  _Port;
		

		public int Port
		{
			get { return _Port; }
			set { _Port = value; }
		}

		// Nothing to do as the culture must be specified in the last engine
		public CultureInfo Culture
		{
			get { return null;}
			set { }
		}

        #region IPersistenceProvider Members


        public Evaluant.Uss.Models.Model Model
        {
            get;
            set;
        }

        #endregion
    }
}
