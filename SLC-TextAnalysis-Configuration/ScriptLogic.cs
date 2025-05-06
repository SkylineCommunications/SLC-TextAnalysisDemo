using System;

internal class ScriptLogic
{
    public string PathSavedFile { get; set; }

    public bool UploadFile(UploadController controller)
    {
        var path = controller.GetFilePath();

        if (string.IsNullOrEmpty(path))
        {
            // no file selected yet
            return false;
        }
        else
        {
            return HandleFile(controller, path);
        }
    }

    private bool HandleFile(UploadController controller, string path)
    {
		// avoids uploading the file again if the filename already exists
		//if (FileHandler.CheckIfFileExists(path))
		//{
		//    controller.NotifyFileAlreadyExists();
		//    return false;
		//}

		// ensure file is secrets.json
		if (!FileHandler.CheckIsSecretsFile(path))
		{
			controller.NotifyFileNotSecretsFile();
			return false;
		}

		PathSavedFile = FileHandler.SaveFile(path);
        return true;
    }

}
