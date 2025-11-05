# Processing PDF documents using AI

## About

This sample package shows how DataMiner Assistant can be integrated to automate steps in a DataMiner powered operational workflow. This allows to facilitate any use-case where you want to interpret unstructured text in an automated way. Examples of **use-cases** are the following:
- **Process PDF documents** to extract parameters from it (implemented in this example)
- Processing order information from unstructured text such as emails
- Processing incident information from unstructured text
- ... (feel free to contact us with use-cases)

The package contains two **applications**:
- **Satellite Parameter Extractor**: uses a predefined prompt to read a set of satellite parameters from PDF files.
- **Interactive File Prompt Tool**: can be used to interact with the LLM model as you would do from the ChatGPT web interface but from within DataMiner. This shows the flexibility to use the LLM from anywhere within DataMiner for virutally any use-case.

> [!IMPORTANT]
> **Data Processing via Azure AI Services**
> 
> This package uses **Azure Document Intelligence Service** and **Azure OpenAI Service**. Important considerations:
> - **Azure Document Intelligence Service**: Extracts text from PDF documents via Azure cloud infrastructure
> - **Azure OpenAI Service (Global Standard Deployment)**: Processes prompts and extracted text using LLM models (e.g., GPT-4o) through a globally distributed service
> - Data is transmitted to Microsoft Azure endpoints for processing
> - The global standard deployment of Azure OpenAI ensures high availability but processes data across Azure's global infrastructure
> - Azure AI Services process data in accordance with Microsoft's data handling policies
> - Ensure uploaded content complies with your organization's data handling policies, security requirements, and applicable regulations (e.g., GDPR, CCPA)
> - Review the [Azure Document Intelligence data privacy documentation](https://learn.microsoft.com/en-us/legal/cognitive-services/document-intelligence/data-privacy-security)
> - Review the [Azure OpenAI data privacy and security documentation](https://learn.microsoft.com/en-us/legal/cognitive-services/openai/data-privacy)


## Key Features

### Combination of OCR & LLM models

The sample package uses a combination of OCR (Optical Character Recognition) & LLM (Large Language Models, e.g. GPT-4o) for the applications described in more detail in the following sections. When a file is uploaded by the user, an automation script will first process the document using OCR and will send the plain text extracted from the file to an LLM together with a prompt for further interpretation. The main reason for this two-step approach is that the APIs for the LLMs are not yet supporting sending over PDF documents as a whole.

![Combination OCR and LLM](./images/AI_processing_archtiecture_hihglevel.png)

### Satellite Feed Ingest

This app uses a predefined prompt in the background to process Satellite parameters from PDF documents. The user simply uploads a PDF file and the system will use the AI tools to process the information in the document and create a new Satellite Feed instance it in [DataMiner Object Models (DOM)](aka.dataminer.services/DOM) (visualized on the right). 

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.png)

### Interactive File Prompt App

This is very basic app which allows the user to write any prompt and upload a file and have it processed by the underlying AI tools. It allows the user to write a prompt and upload a file and the tool will process the file and prompt together using the AI tools described [above](#combination-of-OCR-LLM-models).

The tool can be used for any use-case, going from simply asking the tool to tell a joke, to uploading a document and asking the tool to get some data from the document (see the example below).

 ![Interactive File Prompt App](./images/pdf_processing_interactive_prompt_tool_prompt.png)

## Prerequisites

- DataMiner version 10.6.1 or higher

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](aka.dataminer.services/Pricing_Usage_Based). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
