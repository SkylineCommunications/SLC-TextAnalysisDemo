# Processing PDF documents using AI

## About

This sample package shows how **AI services**, such as OCR (Optical Character Recognition) & LLM (Large Lanaguage Models, e.g. GPT-4o) can be integrated with DataMiner to automate steps in a DataMiner powered operational workflow. This allows to facilitate any use-case where you want to interpret unstructured text in an automated way. Examples of **use-cases** are the following:
- **Process PDF documents** to extract parameters from it (implemented in this example)
- Processing order information from unstructured text such as emails
- Processing incident information from unstructured text
- ... (feel free to contact us with use-cases)



The package contains two **applications**:
- **Satellite Parameter Extractor**: uses a predefined prompt to read a set of satellite parameters from PDF files.
- **Interactive File Prompt Tool**: can be used to interact with the LLM model as you would do from the ChatGPT web interface but from within DataMiner. This shows the flexibility to use the LLM from anywhere within DataMiner for virutally any use-case.


> [!NOTE]
> The package relies on external AI services that need to be configured in your own cloud environment. After configuring the AI services, the necessary API keys need to be configured in DataMiner to enable the functionality of the sample package. More information can be found in our [Docs](https://docs.dataminer.services/index.html).

> [!NOTE]
> For non-productions trials, feel free to contact [Skyline Product Marketing](mailto:team.product.marketing@skyline.be) to get secrets to connect to pre-configured AI services by Skyline.

> [!NOTE]
> Skyline is planning to make LLM models available out-of-the-box for any cloud connected DataMiner system. However, release date for this is yet to be determined. For more information, please reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).


## Key Features

### Combination of OCR & LLM models

The sample package uses a combintation of OCR (Optical Character Recognition) & LLM (Large Lanaguage Models, e.g. GPT-4o) for the applications described in more detail in the following sections. When a file is uploaded by the user, an automation script will first process the document using OCR and will send the plain text extracted from the file to an LLM together with a prompt for further interpretation. The main reason for this two-step approach is that the APIs for the LLMs are not yet supporting sending over PDF documents as a whole.

![Combination OCR and LLM](./images/AI_processing_archtiecture_hihglevel.png)

### Satellite Feed Ingest

This app uses a predefined prompt in the background to process Satellite parameters from PDF documents. The user simply uploads a PDF file and the system will use the AI tools to process the information in the document and create a new Satellite Feed instance it in [DataMiner Object Models (DOM)](aka.dataminer.services/DOM) (visualized on the right). 

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.pNg)

### Interactive File Prompt App

This is very basic app which allows the user to write any prompt and upload a file and have it processed by the underlying AI tools. It allows the user to write a prompt and upload a file and the tool will process the file and prompt together using the AI tools described [above](#combination-of-OCR-LLM-models).

The tool can be used for any use-case, going from simply asking the tool to tell a joke, to uploading a document and asking the tool to get some data from the document (see the example below).

 ![Interactive File Prompt App](./images/pdf_processing_interactive_prompt_tool_prompt.png)

## Prerequisites

- DataMiner version 10.5.8 or higher

> [!NOTE]
> The embedded PDF in the Satellite Parameter Extractor Low-Code application will only work on specific browsers (Firefox will work for example, Google Chrome and Edge typically will typically not work). This is due to a security feature blocking some pdf viewers from certai browsers when embedded in a Low-Code application. It is on the backlog for Low-Code applications to make whitelisting for these pdf viewers configurable. However, there is not commitment in terms of release date. For more information, please reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](aka.dataminer.services/Pricing_Usage_Based). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
