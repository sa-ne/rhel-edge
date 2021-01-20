# RHEL Edge Playbooks & Demos

This repository contains a collection of RHEL Edge playbooks and demos. It is currently a work in progress.

# Roles

## hosts

Configure a basic host file based on a list. Define the variable hostfile in your vault and add name/ip pairs for each host file you want defined.

Example:

```yaml
hostfile:
  - name: rhel8-edge-1
    ip: 10.0.133.1
  - name: rhel8-edge-2
    ip: 10.0.133.2
  - name: elasticsearch
    ip: 10.0.133.3
```

## rsyslog

Configure rsyslog to forward logs to an Elasticsearch instance. The following variables (with examples) determine how rsyslog is configured:

```yaml
rsyslog_elasticsearch_error_file: /var/lib/rsyslog/es-errors.log
rsyslog_elasticsearch_allow_unsigned_certs: "on"

elasticsearch_server: elasticsearch
elasticsearch_port: 31434
elasticsearch_user: elastic
elasticsearch_password: password
elasticsearch_search_index: rsyslog-index
```

# Running the Edge Playbook

## Clone Repository

Clone this repository as follows:

```shell
$ git clone https://github.com/sa-ne/rhel-edge.git
```

## Create an Inventory File of Edge Systems

First we need to create an inventory file (in this example at the root of the repository in `local/hosts.yaml`). An example is provided below:

```yaml
all:
  hosts:
    rhel8-edge-1:
      ansible_host: 10.0.133.1
    rhel8-edge-2:
      ansible_host: 10.0.133.2
```

## Create a Vault

Create a vault (in this example at the root of the repository in `local/vault.yaml`) with the necessary to run each role.

```shell
$ ansible-vault create local/vault.yaml
```

## Running the Edge Playbook

Run the playbook as follows:

```shell
$ ansible-playbook --ask-vault-pass -i local/hosts.yaml -e @local/vault.yaml -u root edge.yaml
```

Each role is tagged appropriately so feel free to use `--tags` or `--skip-tags` for the desired effect (see `edge.yaml` for details).

# Demo Application 1 (dotnet core 2.1)

The application is a dotnet core 2.1 that only exposes 3 metrics so prometheus can scrape (`prom_ok`, `prom_warning`, `prom_exception`), it runs using the builder image based on `ubi8`. To deploy the application manually you can simply run:

```shell
$ oc new-app dotnet:2.1-ubi8~https://github.com/sa-ne/rhel-edge  --context-dir=app -l app=demo --name=demo
```

Then you can expose the service and get an external url:

```shell
$ oc expose svc/demo
```

# Demo Application 2 (quarkus with native build)

This demo application is a very simple microservice which offers one REST endpoint that serves for determining whether a number is prime.

In order to build you need to install GraalVM and export the correct environment variables. You can download GraalVM [here](https://github.com/graalvm/graalvm-ce-builds/releases/) or if you running on OSX:

```shell  
brew install --cask  graalvm/tap/graalvm-ce-java11
```

Then export the `JAVA_HOME` variable:

```shell
 export JAVA_HOME=/Library/Java/JavaVirtualMachines/graalvm-ce-java11-20.3.0/Contents/Home
```

If you are running on OSX you need to remove the quarantine attribute from the bits before you can use them:

```shell
xattr -r -d com.apple.quarantine /Library/Java/JavaVirtualMachines/graalvm-ce-java11-20.3.0
```

In order to build the native package, you first need to clone this repository and run withing the `quarkus` directory:

```shell
mvn package -Pnative -DskipTests -Dquarkus.native.container-runtime=podman
```
