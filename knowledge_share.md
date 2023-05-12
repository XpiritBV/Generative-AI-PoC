Explain the example of how a company wants to use a LLM to answer questions about their data.
    pfizer
    Having a Doctor ask a question about a new drug and getting the answer from the LLM. instead of having to read the entire 100 page document.

# LLM and how to add data that is not in the original training
    1. Retrain your LLM to include your data
       Training a LLM is very expensive and time consuming, if your data changes often, this is not a viable option.
    2. Include all the source data you want to ask a question about into the prompt.
       the size of the context of an LLM is, currently, quite limited. 5k for GPT-3 and 1k for GPT-2. There are models that expand that to 34k, but that is still not a lot, and those are not available to the public.
    3. Embedding, this is a hack on 2 using mathematical magic.

# Embedding

Why send a whole document when you only need a single paragraph to answer a question.

# Vectors

Explain what vectors are and how they are used in NLP. And how we can use them to only get the relevant paragraphs for the question we are asking.

# Ingestion

We use a specialize LLM that has the purpose of converting language into vectors.

1. Convert the source document into plain text
   Log of magic here, individual document shape matters for the result. (chapter, paragraph, sentence, tables, etc.) Azure Form Recognizer or other tools are being developed for this specific purpose.
2. Run the text (chuncks) through the LLM to create vectors.
3. Store the vectors in a database so that we can later query this database for the vectors we need to answer a question.

# Asking the question

1. Now that we have a database of vectors of the documents, when we also vectorize the question we can use mathamagics to find parts of the documents that might contain relevant information. Think of it as a smart CTRL+F it's not just exact match, but also contextually matching
"@@bedenk een voorbeld van een synoniem waarop gezocht wordt in een stukje tekst"

2. Prompt engineering.
Now that we have this short list of relevant data we can construct a prompt that we can ask a LLM specifically designed for human conversation answering questions to answer our question.

* temperature
* welke LLM, ChatGPT is heel sterk in menselijke communicatie daarom gebruiken we die voor het antwoorden op vragen. 
    - een tweeling, 1tje sluit je op in de bibliotheek en de andere krijgt een normale opvoeding.
    Stel de zelfde vraag aan hun beide, 1 weet het antwoord, maar kan het niet goed vertellen.
    De andere kan prima een sprekvoeren, maar weet misschien niet 100% het juiste antwoord.

Example of a prompt.
```
You are a specialist doctor.
Your task is to assist other doctors find information about medical guidelines. The medical guidelines are defined by the following set of snippets identified by numbers in the form [1].  
------------  
SNIPPETS  
{snippets}  
------------  
Your answer must be based solely on the SNIPPETS above. Every part of the answer must be supported only by the SNIPPETS above. If the answer consists of steps, provide a clear bullet point list. If you don't know the answer, just say that you don't know. Don't try to make up an answer. Be clear and concise and provide one final answer. NEVER provide questions in the answer.

Provide the answer as a LIST of JSON formatted dictionaries with the following keys:
- "answer_sentence": str, // the answer in your own words
- "snippet_id": int,  // the snippet your answer is based on
- "relevant_substring": str, // a direct quote from the snippet that was most relevant in creating your answer. Use ellipses ... for substrings longer than 10 words.
  
QUESTION: {question}?
```

# Demo. ingestion / Question answering

    Do the demo.

# How to do this in the cloud?

We don't know, but we want to try (show the azure architecture diagram) and go from there.

# In closing

- Link naar repo.
- Link naar Slack channels.
- All is python, we have seen possible ways to do things in C#, but that is lacking.
- privacy
- answers are not always correct
