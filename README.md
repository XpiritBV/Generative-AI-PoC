[
    ![Open in Remote - Containers](
        https://img.shields.io/static/v1?label=Remote%20-%20Containers&message=Open&color=blue&logo=visualstudiocode
    )
](
    https://vscode.dev/redirect?url=vscode://ms-vscode-remote.remote-containers/cloneInVolume?url=https://github.com/XpiritBV/Generative-AI-PoC
)

# Definitions

## Smart

When someone knows a lot about a subject but doesn't have any experience using that information.

## Wise

When someone knows a lot about a subject but also has experience applying that knowledge in real life.

## Ctrl + F

When we talk about Ctrl + F we're talking about a model that is only smart but not wise.
The model is like a representation of a piece of knowledge, without any context.

# Scope of PoC v1

Our first goal is to create a PoC that follows the Crl + F model.  
Our data set will be a book in .pdf format.

## What is out of scope?

- We don't want to spend a lot of time in data preprocessing.

## Technical

![llm-chatbot-embedding-database](https://user-images.githubusercontent.com/7449547/235882195-766d157f-90e7-4f1f-abaa-08131b36cef4.jpg)
[Source of image](https://bdtechtalks.com/2023/05/01/customize-chatgpt-llm-embeddings/)  
We will be looking in the direction of Azure to find services that can help us achieve our PoC.  

## Use cases

In what way can PoC v1 provide real value to people?

# Considerations

Because the Azure OpenAI service only accepts English, we will be limited to only use English.
Maximum amount of 2048 tokens are accepted by the OpenAI embeddings API.

# Investigation 1

In trying to understand how we can expose large sets of data to an LLM without exceeding its token limit, we formulated the following hypothesis.

In the example of a book, we generate embedding vectors for the contents of this book and use a model, in this case 'text-embedding-ada-002', respecting the model's limitation of 2048 tokens (around 2 to 3 pages of text), and replace line endings with spaces. We then store the resulting embedding vectors in a vector database; we are still looking for which one. As our understanding now stands, the context of a question to the model is limited. These embedding vectors offer a shortcut where we can limit the context of the entire book to just the parts that match the generated vectors of the question asked. After which, we can add the required context to the LLM to answer the question.