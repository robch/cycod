#!/usr/bin/env node

import { Command } from 'commander';
import { ConfigCommand } from '../commands/ConfigCommand';
import { PromptCommand } from '../commands/PromptCommand';
import { ChatCommand } from '../commands/ChatCommand';

const program = new Command();

program
  .name('cycodjs')
  .description('CYCODEV CLI - AI-powered command-line interface')
  .version('1.0.0');

program.addCommand(ConfigCommand.createCommand());
program.addCommand(PromptCommand.createCommand());
program.addCommand(ChatCommand.createCommand());

program.parse(process.argv);