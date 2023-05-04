import openai
import os
import re
import requests
import sys
from num2words import num2words
import os
import pandas as pd
import numpy as np
from openai.embeddings_utils import get_embedding, cosine_similarity
import tiktoken

API_KEY = "b447e89fdad44f55aff50b82e90f3be4"

openai.api_type = "azure"
openai.api_key = API_KEY
openai.api_base = "https://rg-openai-sandbox.openai.azure.com/"
openai.api_version = "2022-12-01"

url = openai.api_base + "/openai/deployments?api-version=2022-12-01" 

file_path = "chapter1.txt"

with open(file_path, "r", encoding="utf-8") as file:
    text = file.read()

text.apply(lambda x : get_embedding(x, engine = 'embedding'))