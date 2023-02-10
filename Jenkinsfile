pipeline {
    agent {
        docker { image 'mcr.microsoft.com/dotnet/sdk:6.0' }
    }

    environment {
        DOTNET_CLI_HOME = '/tmp/DOTNET_CLI_HOME'
    }

    stages {
        stage('Cleanup') {
            steps {
                sh 'rm -r ./packages || true'
                sh 'dotnet clean || true'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build ./DarkLink.RoslynHelpers.AttributeGenerator.sln --configuration Release'
            }
        }

        stage('Pack') {
            steps {
                sh 'dotnet pack --no-build ./DarkLink.RoslynHelpers.AttributeGenerator.sln --output ./packages --version-suffix $(date +%s) --configuration Release'
                sh 'dotnet pack --no-build ./DarkLink.RoslynHelpers.AttributeGenerator/DarkLink.RoslynHelpers.AttributeGenerator.csproj --output ./packages --version-suffix $(date +%s) -p:PackageId=DarkLink.RoslynHelpers.AttributeGenerator.Bootstrap --configuration Release'
            }
        }

        stage('Publish') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'private-nuget-repo', passwordVariable: 'apiKey', usernameVariable: 'source')]) {
                    sh "dotnet nuget push ./packages/* --skip-duplicate --source $source --api-key $apiKey"
                }
            }
        }
    }
}
