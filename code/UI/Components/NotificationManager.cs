using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Components.NotificationManager
{
	public enum NotificationType
	{
		Info,
		Success,
		Warning,
		Error,
	}

	public class Notification : IEquatable<Notification>
	{
		public int id;
		public Panel content;
		public string message = "";
		public string icon = "";
		public float timer = 1f;
		public TimeSince OverTimer = 0f;
		public NotificationType type = NotificationType.Info;

		public bool Equals( Notification other )
		{
			return content == other.content;
		}
	}

	public class NotificationManagerUI : Panel
	{
		public NotificationManagerUI()
		{
			AddClass( "notification-manager" );
			StyleSheet.Load( "/UI/Components/NotificationManager.scss" );
		}

/*		public NotificationManagerUI( string SCSSpath) : base( )
		{
		}*/

		/// <summary>
		/// Create a notification using a list : var notification = new Notification();
		/// </summary>
		/// <param name="notification"></param>
		public void AddNotiffication( Notification notification )
		{
			AddChild( new NotificationEntry( notification ) );
		}
		/// <summary>
		/// Create a notification with a message
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		/// <param name="timer"></param>
		public void AddNotiffication( string message, NotificationType type, float timer = 5f )
		{
			var notification = new Notification();
			notification.type = type;
			notification.timer = timer;
			notification.message = message;

			AddNotiffication( notification );
		}
		/// <summary>
		/// Create a notification with a panel
		/// </summary>
		/// <param name="panel"></param>
		/// <param name="type"></param>
		/// <param name="timer"></param>
		public void AddNotiffication( Panel panel, NotificationType type, float timer = 5f )
		{
			var notification = new Notification( );
			notification.type = type;
			notification.timer = timer;
			notification.content = panel;

			AddNotiffication( notification );
		}
	}
	public class NotificationEntry : Panel
	{
		public NotificationEntry( Notification notification )
		{
			AddClass( "notification" );
			AddClass( notification.type.ToString().ToLower() );

			if ( notification.message != "")
			{
				Add.Label( notification.message, "message" );
			}
			if ( notification.content != null )
			{
				AddChild( notification.content );
			}

			_ = LifeTimer( notification.timer );
		}

		public async Task LifeTimer( float timer )
		{
			await Task.DelaySeconds( timer );
			this?.Delete();
		}
	}
}
