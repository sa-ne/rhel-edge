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

# Demo Application

The application is a dotnet core 2.1 that only exposes 3 metrics so prometheus can scrape (`prom_ok`, `prom_warning`, `prom_exception`), it runs using the builder image based on UBI8. To deploy the application manually you can simply run:

```shell
$ oc new-app dotnet:2.1-ubi8~https://github.com/sa-ne/rhel-edge  --context-dir=app -l app=demo --name=demo
```

Then you can expose the service and get an external url:

```shell
$ oc expose svc/demo
```
