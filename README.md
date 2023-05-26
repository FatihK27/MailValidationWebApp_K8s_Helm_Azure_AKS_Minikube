### You can use this repo in order to create Kubernetes cluster and deploy stack that includes Truemail, Postgres, Rabbitmq, PGAdmin, Mail validation web app and mail validation worker service with helm package manager in linux container.



Prerequisites:

**Docker Desktop (You have many virtualization engine options with minikube such as Hyper-V, VirtualBox etc.)**

[Docker Windows Install](https://docs.docker.com/desktop/install/windows-install/)

[Docker Linux Install](https://docs.docker.com/desktop/install/linux-install/)

[Docker MacOS Install](https://docs.docker.com/desktop/install/mac-install/)

**Chocolatey (The Package Manager for Windows)**

https://chocolatey.org/

**Homebrew (The Package Manager for MacOS/Linux) **

https://brew.sh/

**Kubernetes (Container orchestration tool)**

https://kubernetes.io/docs/tasks/tools/install-kubectl-linux/

https://kubernetes.io/docs/tasks/tools/install-kubectl-macos/

https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/

https://community.chocolatey.org/packages/kubernetes-cli

https://formulae.brew.sh/formula/kubernetes-cli
```
choco install kubernetes-cli
```

**Minikube (Minikube is an open-source tool that enables you to run a single-node Kubernetes cluster on your local machine.)**
https://formulae.brew.sh/formula/minikube
https://community.chocolatey.org/packages/Minikube
   ```
   choco install minikube
   minikube start (create a cluster node)
   ```
   
   
   Ingress Controller:
   ```
   minikube addons enable ingress
   minikube addons enable ingress-dns
   ```

**Helm (The package manager for Kubernetes)**

https://community.chocolatey.org/packages/kubernetes-helm

https://formulae.brew.sh/formula/helm#default
```
choco install kubernetes-helm
Helm package Helm
helm install mva helm-0.1.0.tgz
```
