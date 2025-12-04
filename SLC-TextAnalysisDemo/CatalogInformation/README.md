# Processing PDF documents using AI

## About

The DataMiner Assistant module is available as a DataMiner Extension Module (DxM) and is responsible for bringing conversational AI into DataMiner.
This sample package shows how DataMiner Assistant can be integrated to automate steps in a DataMiner powered operational workflow by interpreting unstructured text in an automated way. This is a valuable and efficient approach for a great lot of **use-cases**. For example:
- **Process PDF documents** to extract parameters from it (implemented in this example)
- Processing order information from unstructured text such as emails
- Processing incident information from unstructured text
- ... (feel free to contact us with use-cases)

The package contains two **applications**:
- **Satellite Parameter Extractor**: uses a predefined prompt to read a set of satellite parameters from PDF files.
- **Interactive File Prompt Tool**: can be used to interact with the LLM model as you would do from any commercially available LLM web interface without having to leave the DataMiner environment. This demonstrates the potential to use an LLM from anywhere within DataMiner for virtually any use-case.

> [!IMPORTANT]
> **Data Processing via Azure AI Services**
> 
> This package uses **Azure Document Intelligence Service** and **Azure OpenAI Service**. Important considerations:
> - **Azure Document Intelligence Service**: Extracts text from PDF documents via Azure cloud infrastructure
> - **Azure OpenAI Service (Global Standard Deployment)**: Processes prompts and extracted text using LLM models (e.g., GPT-4o) through a globally distributed service
> - Data is transmitted to Microsoft Azure endpoints for processing. All data is encrypted while in transit. Azure AI Services process data in accordance with Microsoft's data handling policies. 
> - When data is sent to the Document Intelligence Service, both input data and analysis results may be temporarily encrypted and stored in an Azure Storage for a maximum of 24h after the operation has completed. Data is guaranteed to never leave the services's region. For the time being, this will be Western Europe for any user.
> - The global standard deployment of Azure OpenAI ensures the highest availability and lowest costs by processing data across Azure's global infrastructure: while temporarily stored data should never leave the designated region, prompts and responses **might be processed in any geographic area**. In this case, the service's region currently is Sweden for every user.
> - It is the user's responsibility to ensure that the uploaded content complies with their organization's data handling policies, security requirements, and applicable regulations (e.g., GDPR, CCPA)
> - It is highly recommended to review the [Azure Document Intelligence data privacy documentation](https://learn.microsoft.com/en-us/legal/cognitive-services/document-intelligence/data-privacy-security) and the [Azure OpenAI data privacy and security documentation](https://learn.microsoft.com/en-us/legal/cognitive-services/openai/data-privacy).


## Key Features

### Combination of OCR & LLMs

DataMiner Assistant uses a combination of OCR (Optical Character Recognition) & LLMs (Large Language Models, e.g. GPT-4o). When a file is uploaded by the user, an automation script will pick the document up and make it available for the DataMiner Assistant. The DxM will process the document, extract the necessary information via an OCR tool, and forward it to an LLM together with the appropriate prompt for further interpretation. The main reason for this two-step approach is that the APIs for the LLMs are not yet supporting sending documents over as a whole.

![Combination OCR and LLM](./images/AI_processing_architecture_highlevel.png)

### Satellite Feed Ingest

This app uses a predefined prompt in the background to process Satellite parameters from PDF documents. The user simply uploads a PDF file and the system will use the AI tools to process the information in the document and create a new Satellite Feed instance in [DataMiner Object Models (DOM)](aka.dataminer.services/DOM) (visualized on the right). 

![Satellite Feed Ingest App](./images/pdf_processing_AI_Satellite_Feed_Ingest.png)

### Interactive File Prompt App

This basic app allows a user to upload a file and have it processed by the DataMiner Assistant according to their own instructions. The custom instructions need to be provided as a prompt, which will be included in the context sent to the LLM, as described [above](#combination-of-OCR-LLM-models).

The provided tool can be used for virtually any use-case, from just asking the LLM to tell a joke, to more complex scenarios involving uploading a document and requesting the extraction of specific data from it (see the example below).

 ![Interactive File Prompt App](./images/pdf_processing_interactive_prompt_tool_prompt.png)

## Prerequisites

- DataMiner version 10.6.1 or higher

## Pricing

The applications part of this package will consume DataMiner credits, based on the level of usage of the apps. The DataMiner credits will be deducted monthly based on the metered usage. More information about the pricing of DataMiner usage-based services can be found in the [DataMiner Pricing Overview](aka.dataminer.services/Pricing_Usage_Based). 

## Support

For additional help or to discuss additional use-cases, reach out to [Skyline Product Marketing](mailto:team.product.marketing@skyline.be).
