## **You can deploy this demo stack to Azure AKS by following these instructions :+1:**
```
az login
```

Declare CLI  Variables

```
RESOURCE_GROUP=your_resource_group_name \
LOCATION=your_location \
CLUSTER_NAME=mycluster \
NODE_COUNT=1 \
NAMESPACE= your_namespace
```
Create Azure Resource Group, AKS Cluster and get the Kubernetes cluster credentials to context
```
az group create -n ${RESOURCE_GROUP} -l ${LOCATION}
az aks create -g ${RESOURCE_GROUP} -n ${CLUSTER_NAME} -c ${NODE_COUNT} --generate-ssh-keys -l ${LOCATION}
az aks get-credentials -g ${RESOURCE_GROUP} -n ${CLUSTER_NAME}
```
Create Kubernetes namespace in order to deployment and set as default
```
kubectl create namespace ${NAMESPACE}
kubectl config set-context --current --namespace=${NAMESPACE}
```

Helm Package (Run this command in the parent folder or change the package path parameter) - This command packages a chart into a versioned chart archive file
```
helm package ./helm/chart/helm --destination ./helm/chart/
```

Deploy Mail Validation Stack Helm Package to Azure AKS Cluster
```
helm install mva ./helm/chart/helm-0.1.0.tgz --namespace ${NAMESPACE}
```

Install Nginx controller to AKS Cluster
```
helm upgrade --install ingress-nginx ingress-nginx \
  --repo https://kubernetes.github.io/ingress-nginx \
  --set controller.service.externalTrafficPolicy=Local \
  --namespace ${NAMESPACE} --create-namespace
```
Get the ingress' external ip
```
EXTERNAL_IP=$(kubectl get ingress webapp-ingress -o jsonpath='{.status.loadBalancer.ingress[0].ip}')
echo "http://$EXTERNAL_IP"
```
:+1:



  
