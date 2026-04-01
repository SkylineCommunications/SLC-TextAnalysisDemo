# Processing PDF documents using AI

## About

This sample package shows how the **DataMiner Document Intelligence** functionality from the DataMiner Assistant can be used in DataMiner Automation to **process unstructured text from documents**, in this case PDFs. By integrating this functionality in automation scripts, users have full flexibility as to how to use this in their DataMiner-powered operational workflow. For more information, see [Document Intelligence](https://aka.dataminer.services/document-intelligence). 

The **DataMiner Assistant** module is available as a DataMiner Extension Module (DxM), and is responsible for incorporating conversational AI into DataMiner. The functionality from the DataMiner Assistant is currently restricted to systems connected to dataminer.services only. See [Prerequisites](#prerequisites) below. No additional setup is required.

> [!NOTE]
> The documents are stored locally on the DataMiner web server in the `Webpages` folder. DataMiner also allows you to store these documents on a network drive using [DOM attachments](https://aka.dataminer.services/1116Mt-HR), which might be preferred in terms of scalability. Other options to store documents in external online drives such as Sharepoint and Google Drive are on the roadmap.

> [!IMPORTANT]
> The **DataMiner Document Intelligence** functionality used in this package uses **Azure Document Intelligence Service** and **Azure OpenAI Service** in the background. For more information on data privacy, see [Document Intelligence](https://aka.dataminer.services/document-intelligence).

## Key Features

### Satellite Parameter Extractor app

The **Satellite Parameter Extractor** app shows an example of a user interface that makes use of DataMiner Document Intelligence in the background. Upload a document in the app, and the automation scripts will then use the Document Intelligence API from the DataMiner Assistant to intelligently find the parameters in the document and map them to the possible values. The scripts will automatically fill in the parameters in the [DOM](https://aka.dataminer.services/DOM) object representing a satellite feed. The app uses a predefined prompt in the background to process the satellite parameters found in the PDF documents.

On the left, you will find a list of all uploaded documents, in the middle of the screen, you will see the PDF you selected, and on the right, you will find all parameters extracted from the document.

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.png)

> [!TIP]
> You could tailor the low-code app to a use case by adding a button to automatically create an SRM booking or job in the Scheduling app to downlink the satellite feed.

### Automation scripts

In the **Automation** module of DataMiner Cube, you can find the automation scripts used in the sample application:

- **SLC-TextAnalysis-Upload**: An interactive automation script that will show the dialog to upload the document and store the document on the server. This script will call **SLC_TextAnalysis_Script**.
- **SLC_TextAnalysis_Script**: An automation script that will contact the DataMiner Document Intelligence API to extract the parameters from the document and create the DOM instance.

### Prompt and sample document

In the **Documents** module in DataMiner Cube, you can find the prompt/context that is used in the automation script and a sample document to upload in the app.

![Cube Documents](./images/AI_processing_cubedocuments.png)

## Prerequisites

- DataMiner version **10.6.1** or higher (which includes the DataMiner Assistant DxM required for DataMiner Document Intelligence)
- DataMiner System [connected to dataminer.services](https://aka.dataminer.services/connect-to-the-cloud).

## Pricing

The Document Intelligence functionality will be metered and billed based on your consumption, while limited (non-production) use of the functionality using this package is free. Please contact your account manager for more information if you want to integrate the functionality into your operations. For more information about the pricing of DataMiner usage-based services, see [Usage-based services](https://aka.dataminer.services/Pricing_Usage_Based).

## Support

For additional help or to discuss additional use cases, please reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
