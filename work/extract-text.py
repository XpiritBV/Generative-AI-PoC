from pypdf import PdfReader

reader = PdfReader("corpus/9781942788294.pdf")
number_of_pages = len(reader.pages)
pages = reader.pages[9:21]

text = ""
for page in pages:
    text += page.extract_text()

text = text.replace('\n', ' ')  # Replaces line-endings with a space

print(text)