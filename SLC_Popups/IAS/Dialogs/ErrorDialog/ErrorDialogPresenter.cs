﻿namespace SLC_Popups.IAS.Dialogs.ErrorDialog
{
	using System;
	using SLC_Popups.IAS.Extensions;

	internal class ErrorDialogPresenter
	{
		#region Fields
		private readonly ErrorDialogView view;

		private readonly ErrorDialogModel model;
		#endregion

		public ErrorDialogPresenter(ErrorDialogView view, ErrorDialogModel model)
		{
			this.view = view ?? throw new ArgumentNullException(nameof(view));
			this.model = model?? throw new ArgumentNullException(nameof(model));

			Init();
		}

		#region Events
		public event EventHandler<EventArgs> Close;
		#endregion

		#region Methods
		public void LoadFromModel()
		{
			view.ErrorMessage.Text = StringExtensions.Wrap(model.ErrorMessage, 100);
		}

		public void BuildView()
		{
			view.Build();
		}

		private void Init()
		{
			view.CloseButton.Pressed += OnCloseButtonPressed;
		}

		private void OnCloseButtonPressed(object sender, EventArgs e)
		{
			Close?.Invoke(this, EventArgs.Empty);
		}
		#endregion
	}
}
