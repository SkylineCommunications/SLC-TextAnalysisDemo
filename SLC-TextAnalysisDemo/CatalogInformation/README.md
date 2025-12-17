# Processing PDF documents using AI

## About

This sample package shows how the **DataMiner Document Intelligence** functionality from the DataMiner Assistant can be used in DataMiner Automation to **process unstructured text from documents**, in this case PDFs (refer to [DataMiner Docs](https://aka.dataminer.services/document-intelligence) for more information). By integrating this in automation scripts, users have full flexibility as to how to integrate this in their DataMiner-powered operational workflow.

The DataMiner Assistant module is available as a DataMiner Extension Module (DxM) and is responsible for bringing conversational AI into DataMiner. The functionality from the DataMiner Assistant is currently restricted to systems connected to dataminer.services only (see [Prerequisites](#prerequisites)). No additional setup is required.

> [!NOTE]
> The documents are stored locally on the DataMiner web server in the Webpages folder. There are other options available in DataMiner to store this on a network drive using [DOM attachments](https://aka.dataminer.services/1116Mt-HR), which might be preferred in terms of scalability. Other features are also on the roadmap to store documents in external online drives such as Sharepoint and Google Drive.

> [!IMPORTANT]
> **Data Processing via Azure AI Services**
>
> The DataMiner Document Intelligence functionality used in this package uses **Azure Document Intelligence Service** and **Azure OpenAI Service** in the background. Please consult the [DataMiner Document Intelligence docs](https://aka.dataminer.services/document-intelligence) for more information on data privacy.

## Key Features

### Satellite Parameter Extractor

The app **Satellite Parameter Extractor** in the package shows an example of a user interface that makes use of DataMiner Document Intelligence in the background. The app allows you to upload a document, and the automation scripts will use the Document Intelligence API from the DataMiner Assistant to intelligently find the parameters in the document and map them to the possible values. The scripts will then automatically fill in the parameters in the DOM ([DataMiner Object Modelling](https://aka.dataminer.services/DOM)) object representing a satellite feed. The app uses a predefined prompt in the background to process the satellite parameters from the PDF documents.

The app shows a list of uploaded documents on the left, and the selected PDF is displayed in the middle of the screen. On the right side of the screen, the extracted parameters from the document are shown.

You could tailor the low-code app to a use case by adding an initial button to automatically create an SRM booking or job in the Scheduling app to downlink the satellite feed.

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.png)

### Automation scripts

In the Automation module in DataMiner Cube, you can find the automation scripts used in the sample application:

- *SLC-TextAnalysis-Upload*: Interactive automation script that will show the dialog to upload the document and store the document on the server. This script will call *SLC_TextAnalysis_Script*.
- *SLC_TextAnalysis_Script*: Script that will use the file and contact the DataMiner Document Intelligence API to extract the parameters from the document and create the DOM instance.

### Prompt and sample document

In the DataMiner Documents App in DataMiner Cube, you can find the prompt/context that is used in the automation script and a sample document to upload in the app.

![Cube Documents](./images/AI_processing_cubedocuments.png)

## Prerequisites

- DataMiner version **10.6.1** or higher (includes the DataMiner Assistant DxM required for DataMiner Document Intelligence)
- DataMiner System [connected to dataminer.services](https://aka.dataminer.services/connect-to-the-cloud).

## Pricing

The applications that are part of this package will consume DataMiner credits because the apps use the DataMiner Document Intelligence API in the background. The consumption will depend on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](https://aka.dataminer.services/Pricing_Usage_Based).

## Support

For additional help or to discuss additional use cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
