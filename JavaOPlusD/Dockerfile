FROM openjdk:8u131-jdk-alpine AS build-env

ENV http_proxy='http://10.3.80.80:3128'
ENV https_proxy='http://10.3.80.80:3128'

RUN apk add --no-cache curl tar bash

ARG MAVEN_VERSION=3.5.2
ARG SHA=707b1f6e390a65bde4af4cdaf2a24d45fc19a6ded00fff02e91626e3e42ceaff
ARG BASE_URL=https://apache.osuosl.org/maven/maven-3/${MAVEN_VERSION}/binaries

RUN mkdir -p /usr/share/maven /usr/share/maven/ref \
  && curl -fsSL -o /tmp/apache-maven.tar.gz ${BASE_URL}/apache-maven-${MAVEN_VERSION}-bin.tar.gz \
  && echo "${SHA}  /tmp/apache-maven.tar.gz" | sha256sum -c - \
  && tar -xzf /tmp/apache-maven.tar.gz -C /usr/share/maven --strip-components=1 \
  && rm -f /tmp/apache-maven.tar.gz \
  && ln -s /usr/share/maven/bin/mvn /usr/bin/mvn

ENV MAVEN_HOME /usr/share/maven
ENV MAVEN_CONFIG "/root/.m2"

COPY maven-settings.xml /root/.m2/settings.xml
VOLUME "$USER_HOME_DIR/.m2"

COPY . ~/api

WORKDIR ~/api

RUN ["mvn", "-B", "package"]

FROM openjdk:8u131-jre-alpine

WORKDIR /app

COPY --from=build-env ~/api/target/myproject-0.0.1-SNAPSHOT.jar ./myproject-0.0.1-SNAPSHOT.jar

EXPOSE 8080

ENTRYPOINT ["java", "-jar", "myproject-0.0.1-SNAPSHOT.jar", "jdbc:oracle:thin:@10.3.80.181:1521:chemspace"]