import { INamedValues } from './INamedValues';
export declare class TemplateHelpers {
    static processTemplate(template: string, values: INamedValues): string;
    private static interpolateVariables;
    private static evaluateCondition;
    static replaceValues(template: string, values: INamedValues): string;
}
//# sourceMappingURL=TemplateHelpers.d.ts.map