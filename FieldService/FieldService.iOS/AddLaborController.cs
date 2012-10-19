//
//  Copyright 2012  Xamarin Inc.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//        http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.using System;
using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FieldService.Utilities;
using FieldService.Data;
using FieldService.ViewModels;

namespace FieldService.iOS
{
	/// <summary>
	/// Controller for manually adding labor to an assignment
	/// </summary>
	public partial class AddLaborController : BaseController
	{
		readonly LaborController laborController;
		readonly AssignmentDetailsController detailController;
		readonly LaborViewModel laborViewModel;
		TableSource tableSource;

		public AddLaborController (IntPtr handle) : base (handle)
		{
			ServiceContainer.Register (this);

			laborController = ServiceContainer.Resolve<LaborController>();
			detailController = ServiceContainer.Resolve<AssignmentDetailsController>();
			laborViewModel = ServiceContainer.Resolve<LaborViewModel>();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			cancel.SetTitleTextAttributes (new UITextAttributes() { TextColor = UIColor.White }, UIControlState.Normal);
			cancel.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);
			
			var label = new UILabel (new RectangleF(0, 0, 80, 36)) { 
				Text = "Labor",
				TextColor = UIColor.White,
				BackgroundColor = UIColor.Clear,
				Font = Theme.BoldFontOfSize (16),
			};
			var labor = new UIBarButtonItem(label);

			var done = new UIBarButtonItem("Done", UIBarButtonItemStyle.Bordered, (sender, e) => {
				laborViewModel
					.SaveLabor (detailController.Assignment, laborController.Labor)
					.ContinueOnUIThread (_ => DismissViewController (true, delegate { }));
			});
			done.SetTitleTextAttributes (new UITextAttributes() { TextColor = UIColor.White }, UIControlState.Normal);
			done.SetBackgroundImage (Theme.BarButtonItem, UIControlState.Normal, UIBarMetrics.Default);
			
			toolbar.Items = new UIBarButtonItem[] {
				cancel,
				new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
				labor,
				new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
				done,
			};

			tableView.Source = 
				tableSource = new TableSource();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			tableSource.Load (laborController.Labor);
		}

		partial void Cancel (NSObject sender)
		{
			DismissViewController (true, delegate {	});
		}

		/// <summary>
		/// The table source - has static cells
		/// </summary>
		private class TableSource : UITableViewSource
		{
			readonly LaborController laborController;
			readonly UITableViewCell typeCell, hoursCell, descriptionCell;
			readonly UILabel type;
			readonly UITextView description;
			LaborTypeSheet laborSheet;
			
			public TableSource ()
			{
				laborController = ServiceContainer.Resolve<LaborController>();

				typeCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				typeCell.TextLabel.Text = "Type";
				typeCell.AccessoryView = type = new UILabel (new RectangleF(0, 0, 200, 36))
				{
					TextAlignment = UITextAlignment.Right,
					BackgroundColor = UIColor.Clear,
				};
				typeCell.SelectionStyle = UITableViewCellSelectionStyle.None;

				hoursCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				hoursCell.TextLabel.Text = "Hours";
				hoursCell.SelectionStyle = UITableViewCellSelectionStyle.None;

				descriptionCell = new UITableViewCell (UITableViewCellStyle.Default, null);
				descriptionCell.AccessoryView = description = new UITextView(new RectangleF(0, 0, 500, 400))
				{
					BackgroundColor = UIColor.Clear,
				};
				descriptionCell.SelectionStyle = UITableViewCellSelectionStyle.None;
			}

			public void Load (Labor labor)
			{
				type.Text = labor.TypeAsString;
				description.Text = labor.Description;
			}

			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return indexPath.Section == 1 ? 410 : 44;
			}

			public override int NumberOfSections (UITableView tableView)
			{
				return 2;
			}
			
			public override int RowsInSection (UITableView tableview, int section)
			{
				return section == 0 ? 2 : 1;
			}

			public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0 && indexPath.Row == 0) {
					laborSheet = new LaborTypeSheet();
					laborSheet.Dismissed += (sender, e) => {
						var labor = laborController.Labor;
						if (laborSheet.Type.HasValue && labor.Type != laborSheet.Type) {
							labor.Type = laborSheet.Type.Value;

							Load (labor);
						}

						laborSheet.Dispose ();
						laborSheet = null;
					};
					laborSheet.ShowFrom (typeCell.Frame, tableView, true);
				}
			}

			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				if (indexPath.Section == 0) {
					return indexPath.Row == 0 ? typeCell : hoursCell;
				} else {
					return descriptionCell;
				}
			}
			
			protected override void Dispose (bool disposing)
			{
				typeCell.Dispose ();
				hoursCell.Dispose ();
				descriptionCell.Dispose ();
				type.Dispose ();
				description.Dispose ();
				
				base.Dispose (disposing);
			}
		}
	}
}