from pypdf import PdfReader

reader = PdfReader("9781942788294.pdf")
number_of_pages = len(reader.pages)
pages = reader.pages[9:21]
#text = page.extract_text()

text = ""
for page in pages:
    text += page.extract_text()

print(text)