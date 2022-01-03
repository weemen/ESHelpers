# ESHelpers - Eventsourcing Helpers for c#
Welcome to the ESHelpers project to make your eventsourcing life easier

## Project information
| Status ||
|---|---|
| Build status  | ![Build status](https://github.com/weemen/ESHelpers/actions/workflows/cicd.yml/badge.svg?branch=master)  |
| Code coverage status | ![Coverage Status](https://coveralls.io/repos/github/weemen/ESHelpers/badge.svg?branch=master) |
| Latest Release  | ![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/weemen/eshelpers)  |
| NuGET version  | ![Nuget release](https://img.shields.io/nuget/vpre/eshelpers)  |
| License | [![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)|

## Why these helpers?
These helpers should make your life easier when building components related to eventsourcing like:

- Stop doing figuring out / copy pasting the same things in every project
- Creating and restoring aggregate roots
- Dealing with situations where you need to comply to GPDR rules like PII
- Dealing with hashed values like passwords

It's just time to build your domains

## What's in it for the future?
- Making processors easy
- Making saga's easy

## Documentation:
### Easy? Well let's go!! How can we start?
- [Building a very simple aggregate](docs/01-building-a-very-simple-aggregate.md)
- [Dealing with sensitive information: Crypto Schredding & Encryption](docs/02-dealing-with-sensitive-information.md)

### Technical Documentation
- Implementing my own eventstore
- Implementing my own cryptostore

## FAQ
- [Which eventstores are currently supported?](#which-eventstores-are-currently-supported)
- [Which stores for encryption are currently supported?](#which-stores-for-encryption-are-currently-supported)
- [What are the best strategies for unittesting?](#What-are-the-best-strategies-for-unittesting)

#### Which eventstores are currently supported?
Currently there is only support for EventStoreDB and there is 
an InMemory store (mainly used for unittesting). Over time I
might build additional support different technologies. If you
have a specials then please leave it at the issue tracker.

#### Which stores for encryption are currently supported?
Currently there is only support for MySQL as a crypto store
however just like with the eventstore support if you have special
requests then please leave an issue on the issue tracker.

#### What are the best strategies for unittesting?
For the Eventstores and the crypto stores there is an InMemory 
alternative. These InMemory versions are perfectly suitable for
this purpose. Sometimes these InMemory versions also have some
extra methods to make testing easier.
