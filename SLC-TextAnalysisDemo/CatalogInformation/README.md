# Processing PDF documents using AI

## About

This sample package shows how cloud AI tools, such as OCR (Optical Character Recognition) & LLM (Large Lanaguage Models, e.g. GPT-4o) can be integrated with DataMiner to automate steps in a DataMiner powered operational workflow. This allows to facilitate any use-case where you want to interpret unstructured text in an automated way. Examples of **use-cases** are the following:
- **Process PDF documents** to extract parameters from it (implemented in this example)
- Processing order information from unstructured text such as emails
- Processing incident information from unstructured text
- ... (feel free to contact us with use-cases)

![Use case](./images/AI_processing_highlevel.png)

The package contains two **applications**:
- **Interactive File Prompt Tool**: can be used to interact with the GPT model as you would do from the ChatGPT web interface but from within DataMiner. This shows the flexibility to use the LLM from within DataMiner for virutally any use-case.
- **Satellite Feed Ingest**: uses a predefined prompt to read a set of parameters from PDF files containing satellite parameters.

> [!CAUTION]
> At the moment, the package will do some dll replacement in the back. This will not be the case in future releases. Proceed with caution and avoid to install this package on production systems. Ideally, you try this package on a standalone [DaaS System](https://docs.dataminer.services/user-guide/Getting_started/DaaS/Creating_a_DMS_on_dataminer_services.html).

> [!NOTE]
> The package relies on external AI services that need to be configured in your own cloud environment. After configuring the AI services, the necessary API keys need to be configured in DataMiner to enable the functionality of the sample package. More information can be found in our [Docs](https://docs.dataminer.services/index.html).


## Key Features

### Combination of OCR & LLM models

The sample package uses a combintation of OCR (Optical Character Recognition) & LLM (Large Lanaguage Models, e.g. GPT-4o) for the applications described in more detail in the following sections. When a file is uploaded by the user, an automation script will first process the document using OCR and will send the plain text extracted from the file to an LLM together with a prompt for further interpretation. 

![Combination OCR and LLM](./images/AI_processing_archtiecture_hihglevel.png)

### Interactive File Prompt App

This is very basic app which allows the user to write any prompt and upload a file and have it processed by AI tools. It allows the user to write a prompt and upload a file and the tool will process the file and prompt together using the AI tools described [above](#combination-of-OCR-LLM-models).

The tool can be used for any use-case, going from asking the tool to tell a joke, to uploading a document and asking the tool to get some data from the document (see the example below).

 ![Interactive File Prompt App](./images/pdf_processing_interactive_prompt_tool_prompt.png)

### Satellite Feed Ingest

This app uses a predefined prompt in the background to process Satellite parameters from PDF documents. The user simply uploads a PDF file and the system will use the AI tools to process the information in the document and create a new Satellite Feed instance from it in [DOM](https://docs.dataminer.services/user-guide/Advanced_Modules/DOM/DOM.html?q=dataminer%20object%20mo). 

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.png)

## Prerequisites

- DataMiner version 10.5.7 or higher

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](https://docs.dataminer.services/dataminer-overview/Pricing/Pricing_Usage_based_service.html). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
