# Smtp Router - v3.0.0

[![nuget](https://img.shields.io/nuget/v/SmtpRouter.Core.svg)](https://www.nuget.org/packages/SmtpRouter.Core/) 
![GitHub release](https://img.shields.io/github/release/olavodias/SmtpRouter.svg)
![NuGet](https://img.shields.io/nuget/dt/SmtpRouter.Core.svg)
![license](https://img.shields.io/github/license/olavodias/SMTPRouter.svg)

The SMTP Router is a intermediate SMTP server useful to intercept messages and route it to another smtp.

It contains a class that acts as Listener, capturing Smtp messages and storing it. Also, it contains a class that acts as a Router, forwarding the previously captured messages to another Smtp Server. 
The criteria to define to which Smtp a message will be forwarded is called Routing Rule. 

This component comes with basic Routing Rules. Creating custom routing rules is planned for a future release.

These are some of the uses of an SMTP Router:
* SMTP Relay Server
* Switch the destination SMTP server based on the incoming message. This is especially useful when you have systems that can only accept one single SMTP configuration and you want to use more than one SMTP.
* Provide SMTP authentication for a system that does not have such feature
* Use Multiple SMTPs to send messages, due to daily our hourly limits defined by the email provider (*in development*)

This is a new implementation of the Smtp Router component, which is now on it's **major version 3**. 
This components replace the first generation of the Smtp Router project, which can still be found under the [legacy repository](https://github.com/olavodias/SMTPRouter.Gen1).

>If you wish to contact the creator of this component, you can make it thru the [Nuget.org](https://www.nuget.org/packages/SmtpRouter.Core/) page or by email [olavodias@gmail.com](mailto:olavodias@gmail.com).

## In this repository

* [SMTP Router Documentation](https://olavodias.github.io/SMTPRouter_v3)
* [Components](#components)
* [Change Log](#change-log)

## Components

The following components are in this repository:

| Component | Type | Description |
| :--- | :-- | :-- |
| SMTPRouter.Abstractions | Nuget Package | Interfaces to be used when expanding the Smtp Router component |
| SMTPRouter.Core | Nuget Package | The Implementation of the Smtp Routing Component |
| SMTPRouter.Listener | Background Service | An implementation of the Smtp Router Core to listen to Smtp Messages |
| SMTPRouter.Router | Background Service | An implementation of the Smtp Router Core to route Smtp Messages |

## Change Log

Major Release 3 is not backwards compatible with Major Release 2. The [legacy SmtpRouter Implementation](https://github.com/olavodias/SMTPRouter.Gen1) will still receive modifications, but nothing significant.
We recommend migrating to Major Release 3.

### Version v3.0.0

* Configuration Files are in JSON Format Now
* Listener and Router are completely separated
* Two Background Workers were created, and can be implemented as services in Windows or Linux
* Implemented a new folder structure
* Modified logic to manipulate files
* Implemented Message Filters
* Implemented multiple attempts when a process fails
* Implemented it fully compatible with Background Services
* Fix problems that could cause the service to crash
* Added Unit Testing
* Added Integrated Testing
* Added CI/CD Pipeline

