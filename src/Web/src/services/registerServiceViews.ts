import { lazy } from 'react';
import { registerServiceView } from './serviceViewRegistry';

const LambdaListView = lazy(() => import('./lambda/LambdaListView'));
const LambdaDetailView = lazy(() => import('./lambda/LambdaDetailView'));
const S3ListView = lazy(() => import('./s3/S3ListView'));
const S3DetailView = lazy(() => import('./s3/S3DetailView'));
const SqsListView = lazy(() => import('./sqs/SqsListView'));
const SqsDetailView = lazy(() => import('./sqs/SqsDetailView'));
const CloudWatchLogsListView = lazy(() => import('./cloudwatch-logs/CloudWatchLogsListView'));
const CloudWatchLogsDetailView = lazy(() => import('./cloudwatch-logs/CloudWatchLogsDetailView'));
const DynamoDbListView = lazy(() => import('./dynamodb/DynamoDbListView'));
const DynamoDbDetailView = lazy(() => import('./dynamodb/DynamoDbDetailView'));
const SecretsManagerListView = lazy(() => import('./secrets-manager/SecretsManagerListView'));
const SecretsManagerDetailView = lazy(() => import('./secrets-manager/SecretsManagerDetailView'));
const SsmParameterStoreListView = lazy(() => import('./ssm-parameter-store/SsmParameterStoreListView'));
const SsmParameterStoreDetailView = lazy(() => import('./ssm-parameter-store/SsmParameterStoreDetailView'));
const SnsListView = lazy(() => import('./sns/SnsListView'));
const SnsDetailView = lazy(() => import('./sns/SnsDetailView'));
const IamListView = lazy(() => import('./iam/IamListView'));
const IamDetailView = lazy(() => import('./iam/IamDetailView'));
const StepFunctionsListView = lazy(() => import('./step-functions/StepFunctionsListView'));
const StepFunctionsDetailView = lazy(() => import('./step-functions/StepFunctionsDetailView'));
const CloudFormationListView = lazy(() => import('./cloudformation/CloudFormationListView'));
const CloudFormationDetailView = lazy(() => import('./cloudformation/CloudFormationDetailView'));
const EventBridgeListView = lazy(() => import('./eventbridge/EventBridgeListView'));
const AcmListView = lazy(() => import('./acm/AcmListView'));
const ApiGatewayListView = lazy(() => import('./apigateway/ApiGatewayListView'));
const ApiGatewayDetailView = lazy(() => import('./apigateway/ApiGatewayDetailView'));
const Route53ListView = lazy(() => import('./route53/Route53ListView'));
const SesListView = lazy(() => import('./ses/SesListView'));
const SchedulerListView = lazy(() => import('./scheduler/SchedulerListView'));
const SchedulerDetailView = lazy(() => import('./scheduler/SchedulerDetailView'));
const CognitoListView = lazy(() => import('./cognito/CognitoListView'));
const CognitoDetailView = lazy(() => import('./cognito/CognitoDetailView'));
const ApiGatewayV2ListView = lazy(() => import('./apigatewayv2/ApiGatewayV2ListView'));
const ApiGatewayV2DetailView = lazy(() => import('./apigatewayv2/ApiGatewayV2DetailView'));

let registered = false;

/**
 * Register every built-in service view exactly once. Imported for its side effect at app start.
 */
export function registerServiceViews(): void {
  if (registered) {
    return;
  }
  registered = true;

  registerServiceView('lambda', { list: LambdaListView, detail: LambdaDetailView });
  registerServiceView('s3', { list: S3ListView, detail: S3DetailView });
  registerServiceView('sqs', { list: SqsListView, detail: SqsDetailView });
  registerServiceView('cloudwatch-logs', {
    list: CloudWatchLogsListView,
    detail: CloudWatchLogsDetailView,
  });
  registerServiceView('dynamodb', { list: DynamoDbListView, detail: DynamoDbDetailView });
  registerServiceView('secrets-manager', { list: SecretsManagerListView, detail: SecretsManagerDetailView });
  registerServiceView('ssm-parameter-store', {
    list: SsmParameterStoreListView,
    detail: SsmParameterStoreDetailView,
  });
  registerServiceView('sns', { list: SnsListView, detail: SnsDetailView });
  registerServiceView('iam', { list: IamListView, detail: IamDetailView });
  registerServiceView('step-functions', {
    list: StepFunctionsListView,
    detail: StepFunctionsDetailView,
  });
  registerServiceView('cloudformation', {
    list: CloudFormationListView,
    detail: CloudFormationDetailView,
  });
  registerServiceView('eventbridge', { list: EventBridgeListView });
  registerServiceView('acm', { list: AcmListView });
  registerServiceView('apigateway', {
    list: ApiGatewayListView,
    detail: ApiGatewayDetailView,
  });
  registerServiceView('route53', { list: Route53ListView });
  registerServiceView('ses', { list: SesListView });
  registerServiceView('scheduler', {
    list: SchedulerListView,
    detail: SchedulerDetailView,
  });
  registerServiceView('cognito', {
    list: CognitoListView,
    detail: CognitoDetailView,
  });
  registerServiceView('apigatewayv2', {
    list: ApiGatewayV2ListView,
    detail: ApiGatewayV2DetailView,
  });
}

registerServiceViews();
