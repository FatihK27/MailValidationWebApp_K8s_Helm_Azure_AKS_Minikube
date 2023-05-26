az login

RESOURCE_GROUP=your_resource_group_name \
LOCATION=your_location \
CLUSTER_NAME=mycluster \
NODE_COUNT=1 \
NAMESPACE= your_namespace

az group create -n ${RESOURCE_GROUP} -l ${LOCATION}
az aks create -g ${RESOURCE_GROUP} -n ${CLUSTER_NAME} -c ${NODE_COUNT} --generate-ssh-keys -l ${LOCATION}
az aks get-credentials -g ${RESOURCE_GROUP} -n ${CLUSTER_NAME}

kubectl create namespace ${NAMESPACE}
kubectl config set-context --current --namespace=${NAMESPACE}

helm upgrade --install ingress-nginx ingress-nginx \
  --repo https://kubernetes.github.io/ingress-nginx \
  --set controller.service.externalTrafficPolicy=Local \
  --namespace ${NAMESPACE} --create-namespace