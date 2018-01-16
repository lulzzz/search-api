FROM alpine:3.6

RUN apk --update add postgresql-client && rm -rf /var/cache/apk/*

COPY . /app

WORKDIR /app
USER postgres

# ENTRYPOINT ["sh"]
ENTRYPOINT ["sh", "./run_file.sh"]