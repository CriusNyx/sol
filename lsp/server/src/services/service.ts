import {
  Connection,
  InitializeParams,
  InitializeResult,
  TextDocuments,
  TextDocumentSyncKind,
} from "vscode-languageserver/node";
import { TextDocument } from "vscode-languageserver-textdocument";
import {
  createTypeContextService,
  TypeContextService,
} from "./typeContextService";
import { semanticTypes } from "../semantics";
import { createSemanticService, SemanticsService } from "./semanticsService";

interface Config {
  hasConfigurationCapability: boolean;
  hasWorkspaceFolderCapability: boolean;
  hasDiagnosticRelatedInformationCapability: boolean;
}

let config: Config;

export interface Service {
  onInitialize?(params: InitializeParams): void;
  destroy?(): void;
}

interface Services {
  typeContextService: TypeContextService;
  semanticsService: SemanticsService;
}

type ServiceClasses = Record<
  keyof Services,
  (connection: Connection, documents: TextDocuments<TextDocument>) => Service
>;

const serviceInitializers: ServiceClasses = {
  typeContextService: createTypeContextService,
  semanticsService: createSemanticService,
};

let serviceInstances: Readonly<Services>;

function registerServices(
  connection: Connection,
  documents: TextDocuments<TextDocument>
) {
  console.log("registering services");
  function createService(
    key: string,
    initializer: (
      connection: Connection,
      documents: TextDocuments<TextDocument>
    ) => void
  ) {
    console.log(`creating service ${key}`);
    return initializer(connection, documents);
  }

  serviceInstances = Object.entries(serviceInitializers).reduce(
    (prev, [key, value]) => ({
      ...prev,
      [key]: createService(key, value),
    }),
    {}
  ) as Readonly<Services>;
}

function initializeServices(params: InitializeParams) {
  const tokenModifiers: string[] = ["none"];

  const capabilities = params.capabilities;

  config = {
    // Does the client support the `workspace/configuration` request?
    // If not, we fall back using global settings.
    hasConfigurationCapability: !!(
      capabilities.workspace && !!capabilities.workspace.configuration
    ),
    hasWorkspaceFolderCapability: !!(
      capabilities.workspace && !!capabilities.workspace.workspaceFolders
    ),
    hasDiagnosticRelatedInformationCapability: !!(
      capabilities.textDocument &&
      capabilities.textDocument.publishDiagnostics &&
      capabilities.textDocument.publishDiagnostics.relatedInformation
    ),
  };

  const result: InitializeResult = {
    capabilities: {
      textDocumentSync: TextDocumentSyncKind.Incremental,
      // Tell the client that this server supports code completion.
      semanticTokensProvider: {
        legend: {
          // Cast because typescript enums have weird encoding.
          tokenTypes: [...semanticTypes],
          tokenModifiers,
        },
        full: true,
        range: false,
      },
    },
  };
  if (config.hasWorkspaceFolderCapability) {
    result.capabilities.workspace = {
      workspaceFolders: {
        supported: true,
      },
    };
  }

  Object.values(serviceInstances).forEach((x) =>
    (x as Service).onInitialize?.(params)
  );

  return result;
}

export const ServiceManager = {
  registerServices,
  initializeServices,
};
