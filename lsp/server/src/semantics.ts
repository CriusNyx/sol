export const SemanticType = {
  Type: "type",
  Variable: "variable",
  Keyword: "keyword",
  Operator: "operator",
  Method: "method",
} as const;
import * as sol from "../../sol-js/sol";
import { SemanticToken } from "../../sol-types/index";

export const semanticTypes = Object.values(
  SemanticType
) as readonly SemanticType[];

export type SemanticType = (typeof SemanticType)[keyof typeof SemanticType];

export function analyzeSemantics(source: string) {
  const semanticTokens = sol.analyze_program_semantics(
    source
  ) as SemanticToken[];
  return semanticTokens
    .filter((x) => x.token_type !== "None")
    .map((x) => ({
      ...x,
      semanticType: SemanticType[
        x.token_type as keyof typeof SemanticType
      ] as SemanticType,
    }));
}
