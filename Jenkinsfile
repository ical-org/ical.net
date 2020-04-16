def releasableBranch = '(?:origin/)?(?:(?:master)|(?:release\\d*))'

pipeline {
    options {
        timestamps()
        ansiColor 'xterm'
    }

    agent {
        node {
            label 'windows && docker'
        }
    }

    environment {
        BUILD_DEBUG = 1
    }

    stages {

        stage('Checkout') {
            steps {
                cleanWs()
                checkout scm
            }
        }

        stage('Build') {
            steps {
                dir('net-core') {
                    powershell '.\\build.ps1'

                }
            }
        }

        stage('Test') {
            steps {
                dir('net-core') {
                    powershell '.\\test.ps1'
                }
            }
        }

        stage('Publish') {
            when {
                beforeAgent true
                expression { env.BRANCH_NAME ==~ releasableBranch }
            }
            steps {

                script {
                    def server = Artifactory.server('DISCO')
                    def spec = """\
                    {
                        "files": [{
                            "pattern": "net-core/Ical.Net/nupkgs/*.nupkg",
                            "target": "nuget-local"
                        }]
                    }
                    """.stripIndent()
                    def buildInfo = server.upload(spec)
                    server.publishBuildInfo(buildInfo)
                }
            }
        }
    }
}
