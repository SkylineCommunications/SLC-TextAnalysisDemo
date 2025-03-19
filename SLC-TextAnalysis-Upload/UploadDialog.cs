using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;

internal class UploadDialog : Dialog
{
    public UploadDialog(IEngine engine) : base(engine)
    {
        Title = "Upload PDF";

        Label label = new Label("Please select a file (.pdf) to upload:");
        label.SetWidthAuto();

        FileSelector = new FileSelector();
        FileSelector.AllowedFileNameExtensions = new[] { ".pdf" };
        FileSelector.AllowMultipleFiles = false;

        // override file name 
        // Label newNameLabel = new Label("Please assign a name to the new image file:");
        // label.SetWidthAuto();

        // NewName = new TextBox();
        // NewName.PlaceHolder = "Insert image name";

        // ImageTypeRadioButtonList = new RadioButtonList(new[] { "Icon (64x64, 1:1)", "Logo (300x100, 3:1)", "Profile Image (128x128 1:1)", "Image" });
        // ImageTypeRadioButtonList.Selected = "Icon (32x32, 1:1)";

        //ImageOptionsRadioButtonList = new RadioButtonList(new[] { "Original", "Custom" });
        //ImageOptionsRadioButtonList.IsVisible = false;
        // ImageOptionsRadioButtonList.Selected = "Original";

        //CustomWidth = new TextBox();
        //CustomWidth.PlaceHolder = "Width (px)";
        //CustomWidth.IsVisible = false;

        //CustomHeight = new TextBox();
        //CustomHeight.PlaceHolder = "Height (px)";
        //CustomHeight.IsVisible = false;

        //KeepAspectRatio = new CheckBox();
        //KeepAspectRatio.Text = "Preserve Aspect Ratio";
        //KeepAspectRatio.IsVisible = false;

        Message = new Label();

        ButtonOK = new Button("OK");
        ButtonOK.Width = 100;
        ButtonCancel = new Button("Cancel");
        ButtonCancel.Width = 100;

        AddWidget(label, 0, 0);
        AddWidget(FileSelector, 1, 0);
        // AddWidget(newNameLabel, 2, 0);
        //AddWidget(NewName, 3, 0);
        //AddWidget(ImageTypeRadioButtonList, 4, 0);
        //AddWidget(ImageOptionsRadioButtonList, 5, 0);
        //AddWidget(CustomWidth, 6, 0);
        //AddWidget(CustomHeight, 7, 0);
        //AddWidget(KeepAspectRatio, 8, 0);
        AddWidget(Message, 9, 0);
        AddWidget(ButtonCancel, 10, 2, HorizontalAlignment.Right);
        AddWidget(ButtonOK, 10, 3);
    }

    public FileSelector FileSelector { get; set; }

    // public TextBox NewName { get; set; }

    public Button ButtonOK { get; set; }

    public Button ButtonCancel { get; set; }

    // public RadioButtonList ImageTypeRadioButtonList { get; set; }

    // public RadioButtonList ImageOptionsRadioButtonList { get; set; }

    // public TextBox CustomWidth { get; set; }

    // public TextBox CustomHeight { get; set; }

    // public CheckBox KeepAspectRatio { get; set; }

    public Label Message { get; set; }
}

