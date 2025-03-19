using System;
using System.IO;
using System.Linq;
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.InteractiveAutomationScript;

internal class UploadController
{
    private IEngine _engine;

    private InteractiveController _controller;

    private UploadDialog _dialog;

    public UploadController(IEngine engine)
    {
        _engine = engine;
        _controller = new InteractiveController(engine);
    }

    public EventHandler<EventArgs> OnOK { get; set; }

    public EventHandler<EventArgs> OnCancel { get; set; }

    public void Run()
    {
        // _controller.Run(_dialog);
		_controller.ShowDialog(_dialog);
    }

    public void BuildDialog()
    {
        _dialog = new UploadDialog(_engine);
        _dialog.ButtonOK.Pressed += OnOK;
        _dialog.ButtonCancel.Pressed += OnCancel;

        // _dialog.ImageTypeRadioButtonList.Changed += ImageTypeRadioButtonChanged;
        // _dialog.ImageOptionsRadioButtonList.Changed += ImageOptionsRadioButtonChanged;
    }

    public string GetFilePath()
    {
        var path = _dialog.FileSelector.UploadedFilePaths;

        _engine.GenerateInformation($"Get File Path from Controller: {path.FirstOrDefault()}");

        if (path.Count() == 0)
        {
            _dialog.Message.Text = "ATTENTION: Do not forget to upload the file before clicking OK.";
            _engine.GenerateInformation($"No file path returned to controller");
            return string.Empty;
        }
        else if (path.Count() > 1)
        {
            _engine.GenerateInformation("More than one file path name returned by dialog to controller");
            return path.First();
        }
        else
        {
            _engine.GenerateInformation($"Got single file path from controller: {path.First()}");
            return path.First();
        }
    }

    public void NotifyFileAlreadyExists()
    {
        _dialog.Message.Text = "ATTENTION: A file with that name already exists. Please assign a different name before uploading";
    }

    public void ToggleOkButton()
    {
        _dialog.ButtonOK.IsEnabled = !_dialog.ButtonOK.IsEnabled;
    }
}
