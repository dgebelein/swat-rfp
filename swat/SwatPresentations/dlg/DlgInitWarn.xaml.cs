using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SwatPresentations
{
	public enum MessageLevel { Error, Warning, Info };

	/// <summary>
	/// Interaktionslogik für DlgInitWarn.xaml
	/// </summary>
	public partial class DlgInitWarn : Window
	{
		private String _messageHeader;
		private String _messageDetail;
		private String _messageBlock;
		private String _messageRequest;


		private MessageLevel _level;
		private bool _retVal;



		public DlgInitWarn()
		{
			InitializeComponent();
		}

		private DlgInitWarn(string messageHeader, String messageDetail, string  messageBlock, string messageRequest, MessageLevel level)
		{
			InitializeComponent();

			_messageHeader= messageHeader;
			_messageDetail = messageDetail;
			_messageBlock = messageBlock;
			_messageRequest = messageRequest;
         _level = level;
			switch (_level)
			{
				case MessageLevel.Error: msgSymbol.Source = new BitmapImage(new Uri(@"/SwatPresentations;component/resources/Images/error.png", UriKind.Relative)); break;
				case MessageLevel.Warning: msgSymbol.Source = new BitmapImage(new Uri(@"/SwatPresentations;component/resources/Images/warning.png", UriKind.Relative)); break;
				case MessageLevel.Info: msgSymbol.Source = new BitmapImage(new Uri(@"/SwatPresentations;component/resources/Images/info.png", UriKind.Relative)); break;
			}
		}

		private void This_Loaded(object sender, RoutedEventArgs e)
		{
			MessageHeader.Text = _messageHeader;
			MessageDetail.Text = _messageDetail;
			MessageBlock.Text = _messageBlock;
			MessageRequest.Text = _messageRequest;

		}

		public static bool Show(string messageHeader, String messageDetail, string messageBlock, string messageRequest, MessageLevel level)
		{

			DlgInitWarn dlg = new DlgInitWarn(messageHeader, messageDetail, messageBlock, messageRequest, level);
			dlg.ShowDialog();
			return dlg._retVal;

		}

		private void CmdYes(object sender, RoutedEventArgs e)
		{
			This._retVal = true;
			This.Close();
		}

		private void CmdNo(object sender, RoutedEventArgs e)
		{
			This._retVal = false;
			This.Close();
		}
	}
}
