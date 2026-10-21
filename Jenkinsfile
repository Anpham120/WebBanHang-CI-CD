pipeline {
    agent any

    options {
        timestamps()
        disableConcurrentBuilds()
        skipDefaultCheckout(true)
    }

    triggers {
        githubPush()
    }

    environment {
        APP_NAME = 'webbanhang-demo'
        APP_IMAGE = 'webbanhang-demo'
        APP_PORT = '18080'
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                sh 'rm -f .previous-image'
            }
        }

        stage('Check Docker') {
            steps {
                sh 'docker version && docker info --format "Docker host: {{.Name}}"'
            }
        }

        stage('Restore') {
            steps {
                sh 'docker build --target restore -f Dockerfile .'
            }
        }

        stage('Build') {
            steps {
                sh 'docker build --target build -f Dockerfile .'
            }
        }

        stage('Test') {
            steps {
                sh 'docker build --target test -f Dockerfile .'
            }
        }

        stage('Publish') {
            steps {
                sh 'docker build --target publish -f Dockerfile .'
            }
        }

        stage('Package Docker Image') {
            steps {
                sh 'docker build --target final -f Dockerfile -t "$APP_IMAGE:$BUILD_NUMBER" .'
            }
        }

        stage('Deploy') {
            steps {
                script { env.DEPLOY_ATTEMPTED = 'true' }
                sh '''#!/bin/sh
set -eu
# Save the old image tag so a failed Verify can roll back.
docker inspect --format '{{.Config.Image}}' "$APP_NAME" > .previous-image 2>/dev/null || : > .previous-image

# Named Docker volume persists the SQLite database across deployments.
docker volume create webbanhang-demo-data >/dev/null

docker rm -f "$APP_NAME" >/dev/null 2>&1 || true
docker run -d \
  --name "$APP_NAME" \
  --restart unless-stopped \
  --cap-drop=ALL --security-opt=no-new-privileges \
  -p "$APP_PORT:8080" \
  -v webbanhang-demo-data:/app/data \
  -e APP_VERSION="$BUILD_NUMBER" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  "$APP_IMAGE:$BUILD_NUMBER"
'''
            }
        }

        stage('Verify') {
            steps {
                script {
                    boolean healthy = false
                    for (int attempt = 1; attempt <= 12; attempt++) {
                        // This checks the new container directly, regardless of
                        // Nginx/host port routing.
                        int result = sh(
                            script: 'docker run --rm --network container:webbanhang-demo curlimages/curl:8.11.1 -fsS --max-time 5 http://127.0.0.1:8080/health',
                            returnStatus: true
                        )
                        if (result == 0) {
                            healthy = true
                            break
                        }
                        sleep time: 3, unit: 'SECONDS'
                    }
                    if (!healthy) error('Verify failed: /health is not reachable. Rolling back if a previous image exists.')
                }
            }
        }
    }

    post {
        success {
            echo 'CI/CD SUCCESS: Application deployed to Ubuntu on TCP port 18080.'
        }
        failure {
            script {
                if (env.DEPLOY_ATTEMPTED == 'true') {
                    sh '''#!/bin/sh
if [ -s .previous-image ]; then
  OLD_IMAGE="$(cat .previous-image)"
  echo "Rolling back to: $OLD_IMAGE"
  docker rm -f "$APP_NAME" >/dev/null 2>&1 || true
  docker run -d \
    --name "$APP_NAME" \
    --restart unless-stopped \
    --cap-drop=ALL --security-opt=no-new-privileges \
    -p "$APP_PORT:8080" \
    -v webbanhang-demo-data:/app/data \
    -e APP_VERSION=rollback \
    -e ASPNETCORE_ENVIRONMENT=Production \
    "$OLD_IMAGE" || true
else
  echo 'No previous deployed image to roll back to.'
fi
'''
                }
            }
        }
    }
}
