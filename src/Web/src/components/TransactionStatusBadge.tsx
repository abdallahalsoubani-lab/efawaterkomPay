interface TransactionStatusBadgeProps {
  status: number;
  statusName: string;
}

function TransactionStatusBadge({ status, statusName }: TransactionStatusBadgeProps) {
  const getStatusClass = () => {
    switch (status) {
      case 0: // Pending
        return 'badge-pending';
      case 1: // Processing
        return 'badge-processing';
      case 2: // Success
        return 'badge-success';
      case 3: // Failed
        return 'badge-failed';
      case 4: // Cancelled
        return 'badge-failed';
      default:
        return '';
    }
  };

  return <span className={`badge ${getStatusClass()}`}>{statusName}</span>;
}

export default TransactionStatusBadge;
