// 
// DialogView.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Jeffrey Stedfast
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.Dialog;
using MonoTouch.UIKit;

namespace FlightLog {
	public class DialogView : UIView
	{
		UITableViewSource source;
		UITableView tableView;
		RootElement root;
		
		public DialogView (RectangleF frame, UITableViewStyle style, RootElement root) : base (frame)
		{
			tableView = MakeTableView (new RectangleF (0, 0, frame.Width, frame.Height), style);
			tableView.BackgroundView.Alpha = 0.0f;
			tableView.AutosizesSubviews = true;
			
			BackgroundColor = UIColor.Clear;
			
			source = new Source (this);
			tableView.Source = source;
			
			AddSubview (tableView);
			
			Root = root;
		}
		
		public DialogView (RectangleF frame, RootElement root) : this (frame, UITableViewStyle.Grouped, root) { }
		
		public RootElement Root {
			get { return root; }
			set {
				if (root == value)
					return;
				
				if (root != null)
					root.Dispose ();
				
				root = value;
				root.TableView = tableView;
				ReloadData ();
			}
		}
		
		public UITableViewStyle Style {
			get { return tableView.Style; }
		}
		
		class Source : UITableViewSource
		{
			DialogView dialog;
			
			public Source (DialogView dialog)
			{
				this.dialog = dialog;
			}
			
			protected RootElement Root {
				get { return dialog.Root; }
			}
			
			public override int NumberOfSections (UITableView tableView)
			{
				return Root.Count;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return Root[section].Count;
			}
			
			public override string TitleForHeader (UITableView tableView, int section)
			{
				return Root[section].Caption;
			}
			
			public override UIView GetViewForHeader (UITableView tableView, int section)
			{
				return Root[section].HeaderView;
			}
			
			public override float GetHeightForHeader (UITableView tableView, int section)
			{
				var view = Root[section].HeaderView;
				
				if (view != null)
					return view.Frame.Height;
				
				return -1.0f;
			}
			
			public override string TitleForFooter (UITableView tableView, int section)
			{
				return Root[section].Footer;
			}
			
			public override UIView GetViewForFooter (UITableView tableView, int section)
			{
				return Root[section].FooterView;
			}
			
			public override float GetHeightForFooter (UITableView tableView, int section)
			{
				var view = Root[section].FooterView;
				
				if (view != null)
					return view.Frame.Height;
				
				return -1.0f;
			}
			
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				return Root[indexPath.Section][indexPath.Row].GetCell (tableView);
			}
			
			public override void WillDisplay (UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
			{
				if (!Root.NeedColorUpdate)
					return;
				
				var colorized = Root[indexPath.Section][indexPath.Row] as IColorizeBackground;
				
				if (colorized != null)
					colorized.WillDisplay (tableView, cell, indexPath);
			}
		}
		
		protected virtual UITableView MakeTableView (RectangleF bounds, UITableViewStyle style)
		{
			return new UITableView (bounds, style);
		}
		
		public void ReloadData ()
		{
			if (root == null)
				return;
			
			root.Prepare ();
			
			tableView.ReloadData ();
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			
			if (tableView != null) {
				tableView.Dispose ();
				tableView = null;
			}
			
			if (source != null) {
				source.Dispose ();
				source = null;
			}
			
			if (root != null) {
				root.Dispose ();
				root = null;
			}
		}
	}
}

