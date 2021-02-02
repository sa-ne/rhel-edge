# RHEL Edge Playbooks & Demos

This repository contains a collection of RHEL Edge playbooks and demos. It is currently a work in progress.

# Ansible Playbooks

## `podman_pull_and_run_image.yml`

### Description 

A playbook that will pull down an image from a registry and create a container based on that image.

## Variables

### Image registry vars
- `image_registry`: String: the URL for the registry to use - can be set through passing in a value for `tower_secret_image_registry` through a Tower survey. 
- `image_registry_username`: String: The username used to authenticate to the image registry - can be set through passing in a value for `tower_secret_image_registry_username` through a Tower Custom Credential.
- `image_registry_password`: String: The token used to authenticate to the image registry - can be set through passing in a value for `tower_secret_image_registry_password` through a Tower Custom Credential.

### Image Vars
- `image_namespace`: String: Namespace of the image in the registry- can be set through passing in a value for `tower_input_image_namespace` through a Tower survey. 
- `image_name`: String: Name of the image in the registry- can be set through passing in a value for `tower_input_image_name` through a Tower survey. 
- `image_tag`: String: Tag of the image in the registry- can be set through passing in a value for `tower_input_image_tag` through a Tower survey. 

### Container Vars
- `container_name`: Name of the container to be created - can be set through passing in a value for `tower_input_container_name` through a Tower survey. 
- `container_privileged`: weather to run the container in privileged mode - can be set through passing in a value for `tower_input_container_privileged` through a Tower survey. defaults to `false` .
- `container_restart_policy`: String: the restart policy set to the container - can be set through passing in a value for `tower_input_container_restart_policy` through a Tower survey. defaults to `always`.


### Dependancies 

#### Roles 
- local role: `podman-pull-and-run-image`
#### collections
- galaxy: `containers.podman`


## `podman_stop_and_remove_image.yml`

### Description 

A playbook that will stop a running container based on its name, and remove the image from the system.

## Variables

### Image Vars
- `image_namespace`: String: Namespace of the image in the registry- can be set through passing in a value for `tower_input_image_namespace` through a Tower survey. 
- `image_name`: String: Name of the image in the registry- can be set through passing in a value for `tower_input_image_name` through a Tower survey. 
- `image_tag`: String: Tag of the image in the registry- can be set through passing in a value for `tower_input_image_tag` through a Tower survey. 

### Container Vars
- `container_name`: Name of the container to be removed - can be set through passing in a value for `tower_input_container_name` through a Tower survey. 


### Dependancies 

#### Roles 
- local role: `podman-stop-and-remove-image`
#### collections
- galaxy: `containers.podman`





## `edge_configure_nodes.yml`

### Description 

A playbook that will call a series of roles to configure a RHEL Edge system for this demo. 
- role `hostname` will configure a basic host file on each system.
- role `rsyslog` will configure rsyslog to forward logs to an Elasticsearch instance. 
- role `pcp` will install and configure openmetrics support.

## Variables

### Image Vars
- `rsyslog_elasticsearch_error_file`: The path for the error file to use
- `rsyslog_elasticsearch_allow_unsigned_certs`: Allow unsigned certificates

- `elasticsearch_server`: URL for the elastic search server - can be set through a Tower Custom Credential.
- `elasticsearch_port`: Port for the elastic search server - can be set through a Tower Custom Credential.
- `elasticsearch_user`: username for the elastic search server - can be set through a Tower Custom Credential.
- `elasticsearch_password`: password for the elastic search server - can be set through a Tower Custom Credential.
- `elasticsearch_search_index`: name of the search index for the elastic search server - can be set through a Tower Custom Credential.


### Dependancies 

#### Roles 
- local role: `hostname`
- local role: `rsyslog`
- local role: `pcp`



## `openshift-create-edge-endpoints.yml`

### Description 

A playbook that will create edge endpoints and corresponding services in a given namespace on openshift for the nodes in the inventory it is run against. 

The playbook uses the `kubeconfig` custom credential in Tower for authentication.

## Variables

- `ocp_rhel_edge_namespace`: Name of the OpenShift namespace to create endpoints and services in - can be set through passing in a value for `tower_input_ocp_rhel_edge_namespace` through a Tower survey.  

### Dependancies 

#### Roles 
- local role: `openshift-create-edge-endpoints`
#### collections
- galaxy: `community.kubernetes`


## `openshift-remove-edge-endpoints.yml`

### Description 

A playbook that will remove edge endpoints and corresponding services in a given namespace on openshift for the nodes in the inventory it is run against. 

The playbook uses the `kubeconfig` custom credential in Tower for authentication.

## Variables

- `ocp_rhel_edge_namespace`: Name of the OpenShift namespace to remove endpoints and services from - can be set through passing in a value for `tower_input_ocp_rhel_edge_namespace` through a Tower survey.  

### Dependancies 

#### Roles 
- local role: `openshift-remove-edge-endpoints`
#### collections
- galaxy: `community.kubernetes`



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

---
**NOTE**

If you are running on OSX, you will need a Linux vm/machine configured to your podman client so you can generate the binary.

---


In order to build the native package, you first need to clone this repository and run withing the `quarkus` directory:

```shell
mvn package -Pnative -DskipTests -Dquarkus.native.container-runtime=podman
```

# Configuring Grafana

We will have grafana showing some metric information from the edge nodes that are publishing using the user-workload prometheus instance on OpenShift. For our custom
Grafana instance we will need to grant the `cluster-monitoring-view` cluster role to the `grafana-serviceaccount`:

```shell
oc adm policy add-cluster-role-to-user cluster-monitoring-view -z grafana-serviceaccount
```

We need a bearer token from this service account to authenticate access to prometheus (configuration inside the prometheus-grafanadatasource.yaml file):

```shell 
oc serviceaccounts get-token grafana-serviceaccount -n ${PROJECT_NAME}
```