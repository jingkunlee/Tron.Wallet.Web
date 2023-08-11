$(function () {
    //nav
    $('.nav-item a[href="' + location.pathname + '"').addClass('active');

    //refresh by online
    var btnRefresh = $(".btn-refresh");

    const refreshBalances = function (eventObj, circulate) {
        if (eventObj.length <= 0) { btnRefresh.removeAttr('disabled').text('刷新资产余额'); return; }
        btnRefresh.attr('disabled', 'disabled').text('正在刷新..');

        var accountId = eventObj.data('account-id');
        var walletId = eventObj.data('wallet-id');

        eventObj.find('td').eq(2).text('请稍后..');
        eventObj.find('td').eq(3).text('请稍后..');
        eventObj.find('td').eq(4).text('请稍后..');
        eventObj.find('td').eq(7).text('请稍后..');

        $.post('/Wallet/GetByOnlineByAccountIdByWalletId', { accountId: accountId, walletId: walletId }, function (result) {
            if (result.status == 2) {
                eventObj.find('td').eq(2).text(result.trxBalance);
                eventObj.find('td').eq(3).text(result.etherBalance);
                eventObj.find('td').eq(4).text(result.createTime);
                eventObj.find('td').eq(7).text(result.updateTime);
            }
            else {
                eventObj.find('td').eq(2).text('0');
                eventObj.find('td').eq(3).text('0');
                eventObj.find('td').eq(4).text('--');
                eventObj.find('td').eq(7).text('--');
            }

            if (circulate) refreshBalances(eventObj.next('[data-account-id]'), true);
            else btnRefresh.removeAttr('disabled').text('刷新资产余额');
        });
    }

    $("table tbody tr[data-account-id]").on('dblclick', function (e) {
        refreshBalances($(e.currentTarget), false);
    });

    //一键刷新
    btnRefresh.click(function () {
        refreshBalances($("table tbody tr[data-account-id]:eq(0)"), true);
    });

    $("#viewPrivateKeyModal").on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget);
        var accountId = button.data('account-id');
        var walletId = button.data('wallet-id');

        var modal = $(this);
        modal.find('.modal-body input[name="AccountId"]').val(accountId);
        modal.find('.modal-body input[name="WalletId"]').val(walletId);
    }).on('keydown', 'input[name="Password"]', function (e) {
        if (e.keyCode !== 13) return true;

        var modalBody = $(this).parent().parent();
        modalBody.find('.alert-danger').hide();

        var accountId = modalBody.find('input[name="AccountId"]').val();
        var walletId = modalBody.find('input[name="WalletId"]').val();
        var password = modalBody.find('input[name="Password"]').val();
        $.post('/Wallet/GetByAccountIdByWalletId', { accountId: accountId, walletId: walletId, password: password }, function (result) {
            if (result.status === 2) {
                $('#viewPrivateKeyModal textarea[name="PrivateKey"]').val(result.privateKey);
                $('#viewPrivateKeyModal input[name="Address"]').val(result.address);
                modalBody.find('.step1').hide();
                modalBody.find('.step2').show();
            }
            else {
                modalBody.find('.alert-danger').show().text(result.message);
            }
        });
    });

    $("select.sel-adress").change(function () {
        var self = $(this);
        self.prev().val(self.val());
    });

    $('#transferOutModal').on('show.bs.modal', function (event) {
        var modal = $(this);
        modal.find('.modal-body .alert-danger,.modal-body .alert-success').remove();

        var button = $(event.relatedTarget);
        var token = button.data('token');
        var accountId = button.data('account-id');
        var walletId = button.data('wallet-id');
        var address = button.data('address');
        var trxBalance = button.data('trx');
        var etherBalance = button.data('ether');

        modal.find('.modal-body input[name="AccountId"]').val(accountId);
        modal.find('.modal-body input[name="WalletId"]').val(walletId);
        modal.find('.modal-body input[name="FromAddress"]').val(address);
        modal.find('.modal-body input[name="Token"]').removeAttr('checked');
        modal.find('.modal-body input[name="Token"][value="1"]').next().text(trxBalance + ' TRX');
        modal.find('.modal-body input[name="Token"][value="2"]').next().text(etherBalance + ' USDT');
        modal.find('.modal-body input[name="Token"][value=' + token + ']').attr('checked', 'checked');
        modal.find('.modal-body input[name="ToAddress"]').val('');

        var amount = '';
        if (token == 1) amount = trxBalance;
        else if (token == 2) amount = etherBalance;

        modal.find('.modal-body input[name="Amount"]').val(amount);
        modal.find('.modal-body input[name="Memo"]').val('');
        modal.find('.modal-body input[name="Password"]').val('');
    }).on('keydown', 'input[name="Password"]', function (e) {
        if (e.keyCode === 13) {
            $(this).parent().parent().parent().next().find('.btn-primary').click();
        }
    }).on('click', '.btn-primary', function () {
        var button = $(this);
        button.attr('disabled', 'disabled').text('请稍后..');

        var modalBody = $(this).parent().prev();
        modalBody.find('.alert-danger,.alert-success').remove();

        var accountId = modalBody.find('input[name="AccountId"]').val();
        var walletId = modalBody.find('input[name="WalletId"]').val();
        var fromAddress = modalBody.find('input[name="FromAddress"]').val();
        var token = modalBody.find('input[name="Token"][checked]').val();
        var toAddress = modalBody.find('input[name="ToAddress"]').val();
        var amount = modalBody.find('input[name="Amount"]').val();
        var memo = modalBody.find('input[name="Memo"]').val();
        var password = modalBody.find('input[name="Password"]').val();

        $.post("/Wallet/TransferOut", { accountId: accountId, walletId: walletId, fromAddress: fromAddress, token: token, toAddress: toAddress, amount: amount, memo: memo, password: password }, function (result) {
            if (result.status === 2) {
                modalBody.append('<div class="alert alert-success" role="alert" style="margin-bottom:0 !important;"><a href="https://tronscan.io/#/transaction/' + result.transactionId + '" target="_blank">' + result.transactionId + '</a></div>');
                button.text('提交成功');

                setTimeout(function () {
                    location.reload();
                }, 10000);
            } else {
                modalBody.append('<div class="alert alert-danger" role="alert" style="margin-bottom:0 !important;">' + result.message + '</div>');
                button.removeAttr('disabled').text('确定');
            }
        });
    });
});