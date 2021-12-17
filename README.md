

<p align="center"><img src="https://cdn.discordapp.com/attachments/768553163270651936/913177117167091732/unknown.png" width="300"></p>





# Project Name
> EasySave is a backup folder software launched by ProSoft.

## Table of Contents
* [General Info](#general-information)
* [Technologies Used](#technologies-used)
* [Built with](#built-with)
* [Features](#features)
* [Setup](#setup)
* [Project Status](#project-status)
* [Created by](#contact)



## General Information
We are the Group 01 of CESI Exia Graduated School and we have just joined the software publisher ProSoft. We are responsible of managing the “EasySave” project, which consists in developing a backup software.

As any software in the ProSoft Suite, EasySave will be integrated into the pricing policy.

- Price per unit: 200 €tax excl.

- Annual maintenance contract 5/7 8-17h (updates included): 12% purchase price (Annual contract with tacit renewal with revaluation based on the SYNTEC index) 

During this project, our team will be responsible of the development, management of major and minor versions, but also documentation for users,
for Customer Support: Information required for Technical Support. To ensure that our work can be taken over by other teams, we must work within certain constraints such as the tools used.
To ensure that our work can be taken over by other teams, we must work within certain constraints such as the tools used. 


## Technologies Used
- C# Programming Language
- .NET Core 5.0 Framework

## Built with
- Visual Studio 2019
- Visual Paradigm (for diagram)
- GitHub 

## Features
EasySave version 1.0
- EasySave version 1.0 is a Console application using .Net Core.
- It allows you to create up to 5 backup jobs.
- The software is usable by English and French speaking users
- The user may request the execution of one of the backup jobs or the sequential execution of all the jobs.
- All items in the source directory are concerned by the backup.
- The software writes in real time in a daily log file the history of the actions of the backup jobs.
- The software records in real time, in a single file, the progress of the backup work.
- The software saves in real time, in a single file, the list of backup tasks
- The files (daily log, status and task list) and configuration files are in JSON format.

EasySave version 2.0
- EasySave version 2.0 is a desktop client application using WPF and .Net Core.
- It allows you to create backup jobs with no limitation.
- The software is able to encrypt the files (extensions defined by the user) using the CryptoSoft software.
- If the presence of business software is detected, the software must prevent the start of a backup job.

EasySave version 3.0
-	EasySave version 3.0 is a desktop client application using WPF and .Net Core.
-	Parallel backup
-	Priority file management: Priority files are files whose extensions are declared by the user in a predefined list.
-	Prohibition to simultaneously transfer files of more n KB so as not to saturate the bandwidth.
-	Real-time interaction with each or all backup works. The user can Pause, Play or Stop each backup job.
-	Temporary pause if the operation of a business software is detected.
-	Remote console: To make it possible to follow in real time the progress of backups on a remote computer, we have developed an interface allowing a user to follow on a remote computer the progress of backup work but also to act on them.
-	Our application is  Single Instance. The application cannot be launched more than once on the same computer.
-	Reduction of parallel jobs if networkload, if the network load is above a threshold, the application will reduce the tasks in parallel so as not to saturate the network.


## GitHub 
To clone this repository use the following command:
```sh
$ git clone https://github.com/nazzim29/G1ProgrammationSysteme.git
```

## Setup
To open the application, download the executable file by following the link:
[EasySave V1.0](https://github.com/nazzim29/G1ProgrammationSysteme/releases/tag/1.0)


For any information please refer to the user guide:
- [User Documentation V2.0](https://github.com/nazzim29/G1ProgrammationSysteme/blob/master/Documentation%20Version%202.0.pdf)
- [User Documentation V3.0](https://github.com/nazzim29/G1ProgrammationSysteme/blob/3.0/Documentation%20Version%203.0.pdf)

## Project Status
- Version 1.0 : Done :white_check_mark: 
- Version 1.1 : In progress :hourglass_flowing_sand:
- Version 2.0 : Done :white_check_mark: 
- Version 3.0 : In progress :hourglass_flowing_sand:


## Created by 
- KADOUCHE Nazim
- BENOUDINA Yasmine
- CHENAOUI Maryam Amira
- BOUNEDJAR Mohamed El Hadi


