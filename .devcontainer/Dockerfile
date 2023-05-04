FROM mcr.microsoft.com/devcontainers/python:0-3.11

SHELL ["/bin/bash", "-c"]

COPY requirements.txt /tmp/
RUN pip install --requirement /tmp/requirements.txt

CMD ["sleep", "infinity"]