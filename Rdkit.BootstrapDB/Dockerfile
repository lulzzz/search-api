FROM alpine:3.6

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'

RUN apk --update add postgresql-client && rm -rf /var/cache/apk/*

COPY . /app

WORKDIR /app
USER postgres

# ENTRYPOINT ["sh"]
ENTRYPOINT ["sh", "./run_file.sh"]