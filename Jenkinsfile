@NonCPS
def showChangeLogs() {
 def changeLogSets = currentBuild.changeSets
 def foo = ""
 for (int i = 0; i < changeLogSets.size(); i++) {
     def entries = changeLogSets[i].items
     for (int j = 0; j < entries.length; j++) {
         def entry = entries[j]
         foo = foo + "${new Date(entry.timestamp)}: ${entry.author}:  ${entry.msg}"
         foo = foo + '<BR>'
         def files = new ArrayList(entry.affectedFiles)
             for (int k = 0; k < files.size(); k++) {
                 def file = files[k]
                 foo = foo + " - ${file.editType.name} ${file.path}"
                 foo = foo + '<BR>'
             }
         foo = foo + '<BR>'
     }
 }
 return foo
}

def sendMail(String mssg){
 emailext (
     subject: "STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]'",
     body: """<p>STARTED: Job '${env.JOB_NAME} [${env.BUILD_NUMBER}]':</p>
     <p>${mssg}</p><p>Check console output at &QUOT;<a href='${env.BUILD_URL}'>${env.JOB_NAME} [${env.BUILD_NUMBER}]</a>&QUOT;</p>""",
     to: "marcel.benders@outlook.de")
}


node ('marcelbenders.de') {
 def mvnHome
 def commitId
 properties([gitLabConnection('GitLab')])
 if(env.BRANCH_NAME != 'master' && env.BRANCH_NAME != 'dev' )
     return 
 stage('preparation') { 
     checkout scm
     commitId = sh(returnStdout: true, script: 'git rev-parse HEAD')
     updateGitlabCommitStatus name: 'restore', state: 'pending', sha: commitId
     updateGitlabCommitStatus name: 'build', state: 'pending', sha: commitId
     updateGitlabCommitStatus name: 'publish', state: 'pending', sha: commitId
     updateGitlabCommitStatus name: 'test', state: 'pending', sha: commitId
     if(env.BRANCH_NAME == 'master'){
         updateGitlabCommitStatus name: 'containerize', state: 'pending', sha: commitId
     }
     updateGitlabCommitStatus name: 'clean', state: 'pending', sha: commitId
 }

 try{
     stage('restore') {
         gitlabCommitStatus("restore") {
             sh 'dotnet restore --configfile ./NuGet.config'
         }
     }
 }catch(Exception ex){
     sendMail("RESULT: ${currentBuild.result}")
     updateGitlabCommitStatus name: 'restore', state: 'failed', sha: commitId
     updateGitlabCommitStatus name: 'build', state: 'canceled', sha: commitId
     updateGitlabCommitStatus name: 'publish', state: 'canceled', sha: commitId
     updateGitlabCommitStatus name: 'test', state: 'canceled', sha: commitId
     if(env.BRANCH_NAME == 'master'){
         updateGitlabCommitStatus name: 'containerize', state: 'canceled', sha: commitId
     }
     updateGitlabCommitStatus name: 'clean', state: 'canceled', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return 
 }
 
 try{
     stage('build'){
         gitlabCommitStatus("build") {
             sh 'dotnet build'
         }
     }
 }catch(Exception ex){
     updateGitlabCommitStatus name: 'build', state: 'failed', sha: commitId
     updateGitlabCommitStatus name: 'publish', state: 'canceled', sha: commitId
     updateGitlabCommitStatus name: 'test', state: 'canceled', sha: commitId
     if(env.BRANCH_NAME == 'master'){
         updateGitlabCommitStatus name: 'containerize', state: 'canceled', sha: commitId
     }
     updateGitlabCommitStatus name: 'clean', state: 'canceled', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return
 }

 try{
     stage('publish'){
         gitlabCommitStatus("publish") {
             sh 'dotnet publish -c Release'
         }
     }
 }catch(Exception ex){
     updateGitlabCommitStatus name: 'publish', state: 'failed', sha: commitId
     updateGitlabCommitStatus name: 'test', state: 'canceled', sha: commitId
     if(env.BRANCH_NAME == 'master'){
         updateGitlabCommitStatus name: 'containerize', state: 'canceled', sha: commitId
     }
     updateGitlabCommitStatus name: 'clean', state: 'canceled', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return
 }

 try{
     stage('test') {
         gitlabCommitStatus("test") {
         }
     }
 }catch(Exception ex){
     updateGitlabCommitStatus name: 'test', state: 'failed', sha: commitId
     if(env.BRANCH_NAME == 'master'){
         updateGitlabCommitStatus name: 'containerize', state: 'canceled', sha: commitId
         updateGitlabCommitStatus name: 'deploy', state: 'canceled', sha: commitId
     }
     updateGitlabCommitStatus name: 'clean', state: 'canceled', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return
 }

 try{
     if(env.BRANCH_NAME == 'master'){
         stage('containerize'){
             gitlabCommitStatus("containerize") {
                 mvnHome = env.BUILD_NUMBER
                     sh "docker build -t docker.qaybe.de/xmlrpc:1.0.${mvnHome} ."
                 withDockerRegistry(credentialsId: 'DockerRegistry', toolName: 'QaybeDocker', url: 'https://docker.qaybe.de') {
                     sh "docker push docker.qaybe.de/xmlrpc:1.0.${mvnHome}"
                 }
             }
         }   
     }
 }catch(Exception ex){
     updateGitlabCommitStatus name: 'containerize', state: 'failed', sha: commitId
     updateGitlabCommitStatus name: 'clean', state: 'canceled', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return
 }

 try{
     stage('clean'){
         gitlabCommitStatus("clean") {
             cleanWs()
         }
     }
 }catch(Exception ex){
     updateGitlabCommitStatus name: 'clean', state: 'failed', sha: commitId
     currentBuild.result = 'FAILURE'
     sendMail("RESULT: ${currentBuild.result}")
     echo "RESULT: ${currentBuild.result}"
     return
 }      

 if(env.BRANCH_NAME == 'master')
 
     stage('notify'){
     def mailText = showChangeLogs()
     sendMail(mailText)
 }
}
